using Azure.AI.OpenAI;
using Azure;
using System.Text.Json;
using OpenAI.Chat;

namespace MCP_POC
{
    public class ChatToolForMCP
    {
        public ChatToolForMCP() { }

        public async Task CallChatTool()
        {
            // Simple MCP Server implementation with Azure OpenAI
            Console.WriteLine("MCP Server with Azure OpenAI Example");
            Console.WriteLine("==========================================");

            // Configuration
            string? endpoint = "https://aifoundrydemo2025-resource.cognitiveservices.azure.com/";
            if (string.IsNullOrEmpty(endpoint))
            {
                Console.WriteLine(" Azure OpenAI endpoint is required");
            }
            string? apiKey = "";
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine(" Azure OpenAI API key is required");
            }
            string? deploymentName = "model-router";

            try
            {
                // Initialize Azure OpenAI client
                var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                var chatClient = azureClient.GetChatClient(deploymentName);

                // MCP Tools - Define available tools
                var weatherTool = ChatTool.CreateFunctionTool(
                    functionName: "get_current_weather",
                    functionDescription: "Get current weather information for a specified city",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "city": {
                                "type": "string",
                                "description": "The name of the city to get weather for"
                            }
                        },
                        "required": ["city"]
                    }
                    """));

                var calculatorTool = ChatTool.CreateFunctionTool(
                    functionName: "calculate_sum",
                    functionDescription: "Calculate the sum of two numbers",
                    functionParameters: BinaryData.FromString("""
                    {
                        "type": "object",
                        "properties": {
                            "number1": {
                                "type": "number",
                                "description": "The first number"
                            },
                            "number2": {
                                "type": "number",
                                "description": "The second number"
                            }
                        },
                        "required": ["number1", "number2"]
                    }
                    """));

                Console.WriteLine("\n MCP Server initialized with tools:");
                Console.WriteLine("   • get_current_weather - Get weather information for a city");
                Console.WriteLine("   • calculate_sum - Calculate sum of two numbers");
                Console.WriteLine("\n Enter your queries (type 'exit' to quit):");

                // Main conversation loop
                while (true)
                {
                    Console.Write("\n> ");
                    string? userInput = Console.ReadLine();

                    if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "exit")
                        break;

                    try
                    {
                        // Create conversation with system message and user input
                        var messages = new List<ChatMessage>
                        {
                            new SystemChatMessage("You are a helpful assistant with access to MCP tools. Use the available tools when appropriate to help the user."),
                            new UserChatMessage(userInput)
                        };

                        var chatOptions = new ChatCompletionOptions
                        {
                            Tools = { weatherTool, calculatorTool }
                        };

                        Console.WriteLine("\n Assistant is working....:");

                        // Get response from Azure OpenAI with function calling
                        var response = chatClient.CompleteChatAsync(messages, chatOptions).Result;


                        // Check if function calls were requested
                        if (response.Value.ToolCalls.Count > 0)
                        {
                            // Add the assistant's message with tool calls to conversation
                            messages.Add(new AssistantChatMessage(response.Value));

                            Console.WriteLine("Processing tool calls...");

                            // Process each tool call
                            foreach (var toolCall in response.Value.ToolCalls)
                            {
                                if (toolCall is ChatToolCall functionCall)
                                {
                                    Console.WriteLine($"   → Calling {functionCall.FunctionName}");

                                    string toolResult = functionCall.FunctionName switch
                                    {
                                        "get_current_weather" => HandleWeatherTool(functionCall.FunctionArguments.ToString()).Result,
                                        "calculate_sum" => HandleCalculatorTool(functionCall.FunctionArguments.ToString()).Result,
                                        _ => "Unknown function"
                                    };

                                    // Add function result to conversation
                                    messages.Add(new ToolChatMessage(functionCall.Id, toolResult));
                                }
                            }

                            // Get final response after function execution
                            var finalResponse = chatClient.CompleteChatAsync(messages, chatOptions).Result;
                            if (finalResponse.Value.Content.Any())
                            {
                                Console.WriteLine(finalResponse.Value.Content.First().Text);
                            }
                            else
                            {
                                Console.WriteLine("Response received but no content available.");
                            }
                        }
                        else
                        {
                            var responseMessage = response.Value.Content.First();
                            // No function calls, just display the response
                            Console.WriteLine(responseMessage.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }

                Console.WriteLine("\nMCP Server session ended.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize: {ex.Message}");
            }
        }


        // MCP Tool Handler 1: Weather Tool
        static async Task<string> HandleWeatherTool(string arguments)
        {
            await Task.Delay(500); // Simulate API call delay

            try
            {
                var args = JsonSerializer.Deserialize<Dictionary<string, object>>(arguments);
                string city = args?["city"]?.ToString() ?? "Unknown";

                Console.WriteLine($"MCP Tool: Getting weather for {city}");

                // Simulated weather data
                var weatherData = new
                {
                    city = city,
                    temperature = Random.Shared.Next(-10, 35),
                    condition = new[] { "Sunny", "Cloudy", "Rainy", "Snowy" }[Random.Shared.Next(4)],
                    humidity = Random.Shared.Next(30, 90),
                    timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(weatherData, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"Weather data retrieved");
                return json;
            }
            catch (Exception ex)
            {
                return $"Error getting weather: {ex.Message}";
            }
        }

        // MCP Tool Handler 2: Calculator Tool  
        static async Task<string> HandleCalculatorTool(string arguments)
        {
            await Task.Delay(100); // Simulate processing delay

            try
            {
                var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
                double number1 = args?["number1"].GetDouble() ?? 0;
                double number2 = args?["number2"].GetDouble() ?? 0;

                Console.WriteLine($"MCP Tool: Calculating {number1} + {number2}");

                var result = new
                {
                    operation = "addition",
                    operands = new[] { number1, number2 },
                    result = number1 + number2,
                    timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"Calculation completed: {number1 + number2}");
                return json;
            }
            catch (Exception ex)
            {
                return $"Error calculating: {ex.Message}";
            }
        }
    }
}
