using Mabean.Abstract;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mabean.Services
{


    public class ChatGptService : IAiService
    {
        private readonly ChatClient _client;
        private readonly List<ChatMessage> _messages = new();

        public ChatGptService(IConfiguration config, string model = "gpt-4o-mini")
        {
            _client = new(model, config["OPENAI_API_KEY"] ?? throw new Exception("OPENAI_API_KEY not found in configuration"));
            _messages.Add(new SystemChatMessage("You are a helpful cybersecurity assistant. You are going to analyze" +
                " a set of security events and classify the action ( the group of events as a whole) as" +
                " Very Low, Low, Mild, Suspicious, Moderate, High, Very High, " +
                "Critical, Extreme, Immediate Threat"));
            _messages.Add(new SystemChatMessage("Your response will be a JSON, wheren one key is the Suspiciousness key where the value is going to be" +
                "the 10 values previously discuessed, and the other it's going to be your analysis with the key Analysis, and the value " +
                "your explanation of why this behavior is like that."));
        }

        public async Task<string> SendMessageAsync(string userMessage)
        {
            _messages.Add(new UserChatMessage(userMessage));

            ChatCompletionOptions options = new()
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 1024,
            };

            ChatCompletion result = await _client.CompleteChatAsync(_messages, options);
            string response = result.Content[0].Text;

            _messages.Add(new AssistantChatMessage(response));
            return response;
        }
    }

}
