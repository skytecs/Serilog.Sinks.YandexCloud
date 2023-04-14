# Serilog.Sinks.YandexCloud
Serilog Sink for Yandex Cloud logging

Setup via appsettings.json:
```
  "Serilog": {
    "Using": [ "Serilog.SinksYandexCloud" ],
    "WriteTo": [
      {
        "Name": "YandexCloud",
        "Args": {
          "Formatter": "Serilog.Formatting.Json.JsonFormatter",
          "FolderId": "<...>",
          "LogGroupId": "<...>",
          "ResourceId": "<...>",
          "ResourceType": "<...>",
          "KeyId": "<...>",
          "ServiceAccountId": "<...>",
          "PrivateKey": "<PRIVATE KEY>"
        }
      }
    ]
  }
```
