using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Protocol.Types;
using System.Collections.Concurrent;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ToolController : ControllerBase
{
    private readonly ConcurrentDictionary<string, ToolDefinition> _toolRegistry;

    public ToolController(ConcurrentDictionary<string, ToolDefinition> toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    [HttpPost]
    public IActionResult AddTool([FromBody] AddToolRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.EndpointUrl))
        {
            return BadRequest("Tool name and endpoint URL are required.");
        }

        if (_toolRegistry.ContainsKey(request.Name))
        {
            return BadRequest($"Tool '{request.Name}' already exists.");
        }

        // Parse InputSchema JSON string to JsonElement
        JsonElement inputSchema;
        try
        {
            inputSchema = JsonSerializer.Deserialize<JsonElement>(request.InputSchemaJson);
        }
        catch (JsonException ex)
        {
            return BadRequest($"Invalid InputSchema JSON: {ex.Message}");
        }

        var tool = new ToolDefinition
        {
            Name = request.Name,
            Description = request.Description,
            InputSchema = inputSchema,
            Annotations = request.Annotations,
            EndpointUrl = request.EndpointUrl
        };

        _toolRegistry.TryAdd(request.Name, tool);
        return Ok($"Tool '{request.Name}' added successfully.");
    }
}

public class ToolDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public JsonElement InputSchema { get; set; }
    public ToolAnnotations Annotations { get; set; }
    public string EndpointUrl { get; set; }
}

// Models/AddToolRequest.cs

public class AddToolRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InputSchemaJson { get; set; } // Accept JSON string
    public ToolAnnotations Annotations { get; set; }
    public string EndpointUrl { get; set; }
}

// Models/CallToolResponse.cs

public class CallToolResponse
{
    public bool IsError { get; set; }
    public JsonElement Content { get; set; }
}

// Models/CallToolResult.cs

public class CallToolResult
{
    public bool IsError { get; set; }
    public JsonElement Content { get; set; }
}


public class ListToolsResult
{
    public List<Tool> Tools { get; set; }
}