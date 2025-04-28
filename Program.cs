using ToolAPI.Services;
using Microsoft.OpenApi.Models;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSingleton<ConcurrentDictionary<string, ToolDefinition>>();
builder.Services.AddSingleton<CallToolHandler>();
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

builder.Services.AddMcpServer()
    .WithHttpTransport() // Placeholder for HTTP transport
.WithListToolsHandler((context, cancellationToken) =>
{
    var toolRegistry = context.Services.GetRequiredService<ConcurrentDictionary<string, ToolDefinition>>();
    var tools = toolRegistry.Values.Select(t => new ModelContextProtocol.Protocol.Types.Tool
    {
        Name = t.Name,
        Description = t.Description,
        InputSchema = t.InputSchema,
        Annotations = t.Annotations
    }).ToList();
    
    return ValueTask.FromResult(new ModelContextProtocol.Protocol.Types.ListToolsResult { Tools = tools });

}).WithCallToolHandler((context, cancellationToken) => 
        context.Services.GetRequiredService<CallToolHandler>().Handler(context, cancellationToken));


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