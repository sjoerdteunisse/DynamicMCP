using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ModelContextProtocol.Protocol.Types;
using System.Collections.Concurrent;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<ConcurrentDictionary<string, ToolDefinition>>();
builder.Services.AddOpenApi();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ToolAPI",
        Version = "v1",
        Description = "API for managing tools"
    });
});


// Configure MCP server with custom handlers
builder.Services.AddMcpServer()
    .WithHttpTransport() // Placeholder for HTTP transport
.WithListToolsHandler(async (context, cancellationToken) =>
{
    var toolRegistry = context.Services.GetRequiredService<ConcurrentDictionary<string, ToolDefinition>>();
    var tools = toolRegistry.Values.Select(t => new ModelContextProtocol.Protocol.Types.Tool
    {
        Name = t.Name,
        Description = t.Description,
        InputSchema = t.InputSchema,
        Annotations = t.Annotations
    }).ToList();
    return new ModelContextProtocol.Protocol.Types.ListToolsResult { Tools = tools };

}).WithCallToolHandler(async (context, cancellationToken) =>
{
    var toolRegistry = context.Services.GetRequiredService<ConcurrentDictionary<string, ToolDefinition>>();
    var tool = toolRegistry.Values.FirstOrDefault(x => x.Name == context.Params.Name);

    var toolName = tool?.Name;
    var inputs = tool?.InputSchema;

    if (tool != null)
    {

        using var httpClient = new HttpClient();
        bool isEmptyInputSchema = tool.InputSchema.GetProperty("properties").EnumerateObject().Any() == false;
        JsonElement responseContent;

        if (isEmptyInputSchema)
        {
            var response = await httpClient.GetAsync(tool.EndpointUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ModelContextProtocol.Protocol.Types.CallToolResponse()
                {
                    IsError = true,
                    Content = new List<Content> { new() { Type = "text", Text = "{\"error\": \"Failed to fetch data\"}" } }
                };
            }

            responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        }
        else
        {
            var response = await httpClient.PostAsJsonAsync(tool.EndpointUrl, inputs, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ModelContextProtocol.Protocol.Types.CallToolResponse()
                {
                    IsError = true,
                    Content = new List<Content> { new() { Type = "text", Text = "{\"error\": \"Failed to post data\"}" } }
                };
            }
            responseContent = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        }

        var content = new Content
        {
            Type = "text",
            Text = JsonSerializer.Serialize(responseContent),
        };

        return new ModelContextProtocol.Protocol.Types.CallToolResponse()
        {
            IsError = false,
            Content = new List<Content> { content }
        };
    }
    throw new Exception($"Tool '{toolName}' not found.");
});



var app = builder.Build();
app.MapMcp();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToolAPI v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the root
    });
}

// Configure the HTTP request pipeline
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
