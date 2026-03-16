using NAudio.Wave;
using System.Text.Json;
using Vosk;

namespace Jarvis.Mark2.Infrastructure
{
    public class VoiceRecognitionService : IDisposable
    {
        private Model? voskModel;
        private VoskRecognizer? recognizer;
        private WaveInEvent? waveIn;

        public event Action<string>? TextRecognized;
        public event Action<string>? ErrorOccurred;
        public event Action<string>? PartialTextRecognized;

        public void StartVoiceRecognition()
        {
            try
            {
                Vosk.Vosk.SetLogLevel(0);

                var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model-ru");

                if (!Directory.Exists(modelPath))
                {
                    ErrorOccurred?.Invoke($"Папка модели не найдена: {modelPath}");

                    return;
                }

                voskModel = new Model(modelPath);
                recognizer = new VoskRecognizer(voskModel, 16000.0f);

                waveIn = new WaveInEvent()
                {
                    DeviceNumber = 0,
                    WaveFormat = new(16000, 1),
                    BufferMilliseconds = 150
                };

                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.RecordingStopped += WaveIn_RecordingStopped;

                waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke("Ошибка запуска распознавания: " + ex.Message);
            }
        }

        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (recognizer is null) return;

            try
            {
                var result = recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded);

                if (result)
                {
                    string json = recognizer.Result();
                    string text = ExtractTextFromJson(json);

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        TextRecognized?.Invoke(text);
                    }
                }
                else
                {
                    string json = recognizer.PartialResult();
                    string text = ExtractPartialTextFromJson(json);

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        PartialTextRecognized?.Invoke(text);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke("Ошибка распознавания: " + ex.Message);
            }
        }

        private void WaveIn_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                ErrorOccurred?.Invoke("Ошибка микрофона: " + e.Exception.Message);
            }
        }

        private static string ExtractTextFromJson(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("text", out JsonElement textElement))
                    return textElement.GetString() ?? string.Empty;
            }
            catch { }

            return string.Empty;
        }

        private string ExtractPartialTextFromJson(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("partial", out JsonElement partialElement))
                    return partialElement.GetString() ?? string.Empty;
            }
            catch { }

            return string.Empty;
        }

        public void Dispose()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            recognizer?.Dispose();
            voskModel?.Dispose();
        }
    }
}
