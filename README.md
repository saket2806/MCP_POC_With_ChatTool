# MCP_POC - Simple Azure OpenAI tools demo (.NET 8)

This sample shows a minimal, console-based "MCP-like" experience using Azure OpenAI function tools in .NET 8.

What it does:
- Starts a console app
- Defines two tools: Temperature of city and add numbers
- Sends your input to Azure OpenAI and executes tools when requested

## Prerequisites
- .NET 8 SDK installed
- An Azure OpenAI resource with:
  - Endpoint URL (e.g., https://<your-resource-name>.cognitiveservices.azure.com/)
  - API key
  - A chat model deployment name (e.g., model-router, gpt-4o-mini)

## 1) Get the code
- Open this folder in your terminal/VS Code/Visual Studio.

## 2) Install packages (commands)
Run these in the project folder (where MCP_POC.csproj is):

- Required
  - dotnet add package Azure.AI.OpenAI --version 2.1.0

- Optional (only if you switch to DefaultAzureCredential later)
  - dotnet add package Azure.Identity --version 1.14.2


## 3) Configure Azure OpenAI settings
In ChatToolForMCP.cs, update the three values:
- endpoint: set to your Azure OpenAI endpoint URL
- apiKey: set to your Azure OpenAI API key
- deploymentName: set to your chat model deployment name (for tools support, use a modern chat-capable model)

Example (replace placeholders):
- endpoint = "https://YOUR-RESOURCE.cognitiveservices.azure.com/"
- apiKey = "YOUR-API-KEY"
- deploymentName = "YOUR-DEPLOYMENT-NAME"

Tip: Do not commit real keys to source control. Prefer environment variables or user-secrets in real apps.

## 4) Build
- dotnet build

## 5) Run
- dotnet run

You should see prompts like:
- "MCP Server initialized with tools:"
- Enter your text, for example:
  - Echo something: echo "hello world"
  - Add numbers: "add 5 and 7"

## Troubleshooting
- 401 Unauthorized: check API key
- 404/BadRequest: check deployment name and that the model supports tools
- Timeouts/Network: check your firewall/proxy and endpoint URL

## Notes
- Target framework: .NET 8
- Packages kept minimal for clarity
- Keep secrets out of source code in real projects