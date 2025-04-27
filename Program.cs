using Microsoft.OpenApi.Models;
using ModelContextProtocol.Protocol.Types;
using System.Collections.Concurrent;

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
    var s = toolRegistry.Values.First(x => x.Name == context.Params.Name);

    var toolName = s.Name;
    var inputs = s.InputSchema;

    if (toolRegistry.TryGetValue(toolName, out var tool))
    {
        using var httpClient = new HttpClient();
        //post with inputs
        var response = await httpClient.GetAsync(tool.EndpointUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);

        var a = new ModelContextProtocol.Protocol.Types.CallToolResponse();
        a.Content = result;
        

        return result;
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
