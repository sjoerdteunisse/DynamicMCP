using System.Collections.Concurrent;
using System.Text.Json;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;

namespace ToolAPI.Services;

public class CallToolHandler
{
    public async ValueTask<ModelContextProtocol.Protocol.Types.CallToolResponse> Handler(RequestContext<CallToolRequestParams> context, CancellationToken cancellationToken)
    {
        var toolRegistry = context.Services.GetRequiredService<ConcurrentDictionary<string, ToolDefinition>>();
        var tool = toolRegistry.Values.FirstOrDefault(x => x.Name == context.Params.Name);

        var toolName = tool?.Name;
        var inputs = context.Params.Arguments;

        if (tool != null)
        {
            using var httpClient = new HttpClient();
            bool isEmptyInputSchema = tool.InputSchema.GetProperty("properties").EnumerateObject().Any() == false;
            JsonElement responseContent;

            if (tool.EndpointUrl.Contains("{"))
            {
                // Handle GET with placeholder replacement
                string dynamicUrl = tool.EndpointUrl;
                var queryParams = new Dictionary<string, string>();

                // Extract input values from Arguments
                if (inputs != null)
                {
                    foreach (var prop in tool.InputSchema.GetProperty("properties").EnumerateObject())
                    {
                        string propName = prop.Name;
                        if (inputs.TryGetValue(propName, out var inputValue))
                        {
                            string value = inputValue.ValueKind switch
                            {
                                JsonValueKind.String => inputValue.GetString(),
                                JsonValueKind.Number => inputValue.GetDouble().ToString(),
                                JsonValueKind.True => "true",
                                JsonValueKind.False => "false",
                                _ => inputValue.ToString()
                            };

                            // Replace placeholder in URL if present
                            string placeholder = $"{{{propName}}}";
                            if (dynamicUrl.Contains(placeholder))
                            {
                                dynamicUrl = dynamicUrl.Replace(placeholder, Uri.EscapeDataString(value));
                            }
                            else
                            {
                                // Add to query parameters
                                queryParams[propName] = value;
                            }
                        }
                    }
                }

                // Append query parameters
                if (queryParams.Any())
                {
                    var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                    dynamicUrl += dynamicUrl.Contains("?") ? "&" : "?";
                    dynamicUrl += queryString;
                }

                // Validate URL
                if (!Uri.TryCreate(dynamicUrl, UriKind.Absolute, out var uri))
                {
                    return new ModelContextProtocol.Protocol.Types.CallToolResponse { IsError = true, Content = new List<Content> { new Content { Type = "text", Text = "{\"error\": \"Invalid URL after placeholder replacement\"}" } } };
                }

                var response = await httpClient.GetAsync(dynamicUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new ModelContextProtocol.Protocol.Types.CallToolResponse { IsError = true, Content = new List<Content> { new Content { Type = "text", Text = "{\"error\": \"Failed to fetch data\"}" } } };
                }

                responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            }
            else if (isEmptyInputSchema)
            {
                // Standard GET for empty schema
                var response = await httpClient.GetAsync(tool.EndpointUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new ModelContextProtocol.Protocol.Types.CallToolResponse { IsError = true, Content = new List<Content> { new Content { Type = "text", Text = "{\"error\": \"Failed to fetch data\"}" } } };
                }

                responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            }
            else
            {
                // POST for non-empty schema without placeholders
                var response = await httpClient.PostAsJsonAsync(tool.EndpointUrl, inputs, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new ModelContextProtocol.Protocol.Types.CallToolResponse { IsError = true, Content = new List<Content> { new Content { Type = "text", Text = "{\"error\": \"Failed to post data\"}" } } };
                }

                responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            }

            var content = new Content { Type = "text", Text = JsonSerializer.Serialize(responseContent) };

            return new ModelContextProtocol.Protocol.Types.CallToolResponse { IsError = false, Content = new List<Content> { content } };
        }

        throw new Exception($"Tool '{toolName}' not found.");
    }
}