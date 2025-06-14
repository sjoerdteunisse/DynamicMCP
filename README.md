# DynamicMCP Server
Dynamic Model Context Protocol API, add tools on the fly and expose them at any time to your MCP clients.



Post examples
```json
 {
    "name": "weather_forecast_berlin_input",
    "description": "Fetches weather forecast for Berlin",
    "inputSchemaJson": "{\"type\":\"object\",\"properties\":{\"latitude\":{\"type\":\"number\"}},\"required\":[\"latitude\"]}",
    "outputSchemaJson": "{\"type\":\"object\",\"properties\":{\"latitude\":{\"type\":\"number\"},\"longitude\":{\"type\":\"number\"},\"current\":{\"type\":\"object\",\"properties\":{\"temperature_2m\":{\"type\":\"number\"},\"wind_speed_10m\":{\"type\":\"number\"}}},\"hourly\":{\"type\":\"object\",\"properties\":{\"temperature_2m\":{\"type\":\"array\",\"items\":{\"type\":\"number\"}},\"relative_humidity_2m\":{\"type\":\"array\",\"items\":{\"type\":\"number\"}},\"wind_speed_10m\":{\"type\":\"array\",\"items\":{\"type\":\"number\"}}}}}}",
    "annotations": {
      "title": "Berlin Weather Forecast",
      "readOnlyHint": true,
      "destructiveHint": false,
      "idempotentHint": true,
      "openWorldHint": false
    },
   "endpointUrl": "https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude=13.41&current=temperature_2m,wind_speed_10m&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m"
  }
```
```json
{
  "name": "weather_forecast_berlin",
  "description": "Fetches current and hourly weather forecast for Berlin from Open-Meteo API",
  "inputSchemaJson": "{\"type\":\"object\",\"properties\":{},\"required\":[]}",
  "outputSchemaJson": "{\"type\":\"object\",\"properties\":{\"latitude\":{\"type\":\"number\"},\"longitude\":{\"type\":\"number\"},\"current\":{\"type\":\"object\",\"properties\":{\"temperature_2m\":{\"type\":\"number\"},\"wind_speed_10m\":{\"type\":\"number\"}}},\"hourly\":{\"type\":\"object\",\"properties\":{\"temperature_2m\":{\"type\":\"array\",\"items\":{\"type\":\"number\"}},\"relative_humidity_2m\":{\"type\":\"array\",\"items\":{\"type\":\"number\"}},\"wind_speed_10m\":{\"type\":\"array\",\"items\":{\"type\":\"number\"}}}}}}",
  "annotations": {
    "title": "Berlin Weather Forecast",
    "readOnlyHint": true,
    "destructiveHint": false,
    "idempotentHint": true,
    "openWorldHint": false
  },
  "endpointUrl": "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41¤t=temperature_2m,wind_speed_10m&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m"
}
```
