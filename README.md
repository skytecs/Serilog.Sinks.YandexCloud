# Serilog.Sinks.YandexCloud
Serilog Sink for Yandex Cloud logging

Setup via appsettings.json:
```
  "Serilog": {
    "Using": [ "Serilog.Sinks.YandexCloud" ],
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
## How to use the sink in gRPC services.
Due to the known issue with the .net gRPC compiler you could encounter the following build error:
```
error CS0433: The type 'AnnotationsReflection' exists in both 'Google.Api.CommonProtos, Version=2.5.0.0, Culture=neutral, PublicKeyToken=3ec5ea7f18953e47' and 'Yandex.Cloud.Protos, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
```
One possible workaround is to add the following nuget package to your service explicitly.

```
    <PackageReference Include="Yandex.Cloud.Protos" Version="1.2.0">
      <Aliases>yandex_protos</Aliases>
    </PackageReference>
```
