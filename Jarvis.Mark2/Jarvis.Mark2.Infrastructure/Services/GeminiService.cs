using Google.GenAI;
using Google.GenAI.Types;

namespace Jarvis.Mark2.Infrastructure.Services
{
    public class GeminiService
    {
        private readonly Client _client;
        private readonly string _modelName;

        public GeminiService(string apiKey, string modelName = "gemini-2.5-flash")
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Gemini API key is empty.", nameof(apiKey));
            }

            _client = new Client(apiKey: apiKey);
            _modelName = modelName;
        }

        public async Task<string> AskAsync(string userText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userText))
            {
                return "повторите пожалуйста";
            }

            var config = new GenerateContentConfig
            {
                Temperature = 0.7f,
                MaxOutputTokens = 250,
                SystemInstruction = new Content
                {
                    Parts =
                    [
                        new()
                        {
                            Text = "Ты голосовой помощник JARVIS в Windows-приложении. " +
                               "Отвечай кратко, понятно и по-русски. " +
                               "Без длинных вступлений. " +
                               "Если вопрос неясен, скажи это прямо."
                        }
                    ]
                }
            };

            var responce = await _client.Models.GenerateContentAsync(
                model: _modelName,
                contents: userText,
                config: config,
                cancellationToken: cancellationToken);

            var text = responce?.Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text;

            return string.IsNullOrWhiteSpace(text)
                ? "запрос неудачный"
                : text;               
        }
    }
}
