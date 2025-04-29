# DynamicMCP
Dynamic Model Context Protocol API



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
