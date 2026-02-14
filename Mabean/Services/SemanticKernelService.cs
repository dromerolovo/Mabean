using Mabean.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Threading.Tasks;

#pragma warning disable SKEXP0070

namespace Mabean.Services
{
    public class SemanticKernelService : IAiService
    {
        private readonly IChatCompletionService _chat;
        private readonly ChatHistory _history = new();

        public SemanticKernelService(IConfiguration config)
        {
            var provider = "gemini";
            var model = "gemini-3-flash-preview";
            var apiKey = config["GEMINI_API_KEY"] ?? throw new System.Exception("GEMINI_API_KEY not found in configuration");

            var builder = Kernel.CreateBuilder();

            switch (provider.ToLower())
            {
                case "ollama":
                    builder.AddOllamaChatCompletion(model, new System.Uri("http://localhost:11434"));
                        break;
                case "gemini":
                    builder.AddGoogleAIGeminiChatCompletion(model, apiKey);
                    break;
            }

            var kernel = builder.Build();
            _chat = kernel.GetRequiredService<IChatCompletionService>();

            _history.AddSystemMessage("You are a helpful cybersecurity assistant. You are going to analyze" +
                " a set of security events and classify the action ( the group of events as a whole) as" +
                " Very Low, Low, Mild, Suspicious, Moderate, High, Very High, " +
                "Critical, Extreme, Immediate Threat");
            _history.AddSystemMessage("Your response will be a JSON, where one key is the SuspiciousnessName key where the value is going to be" +
                "one of the 10 values previously discuessed, and the other it's going to be your analysis with the key Analysis, and the value: " +
                "your explanation of why this behavior is like that. Don't consider the sandbox environment or Mabean.exe or the different dlls for your analysis, just focus on the behavior");
        }

        public async Task<string> SendMessageAsync(string userMessage)
        {
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                if (_history[i].Role != AuthorRole.System) _history.RemoveAt(i);
            }

            _history.AddUserMessage(userMessage);

            var response = await _chat.GetChatMessageContentAsync(_history);
            var text = response.Content ?? string.Empty;

            Console.WriteLine($"AI Response: {text}");

            //_history.AddAssistantMessage(text);
            return text;
        }
    }
}
