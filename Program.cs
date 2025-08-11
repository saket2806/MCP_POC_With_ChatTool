using Azure;
using Azure.AI.OpenAI;

using MCP_POC;

using OpenAI.Chat;

using System.Text.Json;

// Simple MCP Server implementation with Azure OpenAI
Console.WriteLine("MCP Server with Azure OpenAI Example");
Console.WriteLine("==========================================");

ChatToolForMCP chatToolCall = new ChatToolForMCP();
await chatToolCall.CallChatTool();
