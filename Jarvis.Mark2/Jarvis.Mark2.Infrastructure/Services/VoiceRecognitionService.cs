using NAudio.Wave;
using System.Text.Json;
using Vosk;

namespace Jarvis.Mark2.Infrastructure.Services
{
    public class VoiceRecognitionService : IDisposable
    {
        private Model? voskModel;
        private VoskRecognizer? recognizer;
        private WaveInEvent? waveIn;

        private readonly object _sync = new();
        private volatile bool _isRecognitionEnabled;
        private volatile bool _isDisposed;

        public event Action<string>? TextRecognized;
        public event Action<string>? ErrorOccurred;
        public event Action<string>? PartialTextRecognized;

        public bool IsListening => waveIn != null && _isRecognitionEnabled;

        public void StartVoiceRecognition()
        {
            lock (_sync)
            {
                if (_isDisposed) return;

                try
                {
                    if (waveIn != null)
                    {
                        _isRecognitionEnabled = true;
                        return;
                    }

                    Vosk.Vosk.SetLogLevel(0);

                    if (voskModel == null)
                    {
                        var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model-ru");

                        if (!Directory.Exists(modelPath))
                        {
                            ErrorOccurred?.Invoke($"Папка модели не найдена: {modelPath}");
                            return;
                        }

                        voskModel = new Model(modelPath);
                    }

                    recognizer?.Dispose();
                    recognizer = new VoskRecognizer(voskModel, 16000.0f);

                    waveIn = new WaveInEvent
                    {
                        DeviceNumber = 0,
                        WaveFormat = new WaveFormat(16000, 1),
                        BufferMilliseconds = 150
                    };

                    waveIn.DataAvailable += WaveIn_DataAvailable;
                    waveIn.RecordingStopped += WaveIn_RecordingStopped;

                    _isRecognitionEnabled = true;
                    waveIn.StartRecording();
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke("Ошибка запуска распознавания: " + ex.Message);
                }
            }
        }

        public void StopVoiceRecognition()
        {
            lock (_sync)
            {
                try
                {
                    _isRecognitionEnabled = false;

                    if (waveIn != null)
                    {
                        waveIn.DataAvailable -= WaveIn_DataAvailable;
                        waveIn.RecordingStopped -= WaveIn_RecordingStopped;

                        waveIn.StopRecording();
                        waveIn.Dispose();
                        waveIn = null;
                    }

                    recognizer?.Dispose();
                    recognizer = null;
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke("Ошибка остановки микрофона: " + ex.Message);
                }
            }
        }

        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (!_isRecognitionEnabled)
                return;

            var localRecognizer = recognizer;
            if (localRecognizer is null)
                return;

            try
            {
                var result = localRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded);

                if (!_isRecognitionEnabled)
                    return;

                if (result)
                {
                    string json = localRecognizer.Result();
                    string text = ExtractTextFromJson(json);

                    if (_isRecognitionEnabled && !string.IsNullOrWhiteSpace(text))
                    {
                        TextRecognized?.Invoke(text);
                    }
                }
                else
                {
                    string json = localRecognizer.PartialResult();
                    string text = ExtractPartialTextFromJson(json);

                    if (_isRecognitionEnabled && !string.IsNullOrWhiteSpace(text))
                    {
                        PartialTextRecognized?.Invoke(text);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isRecognitionEnabled)
                {
                    ErrorOccurred?.Invoke("Ошибка распознавания: " + ex.Message);
                }
            }
        }

        private void WaveIn_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (!_isRecognitionEnabled)
                return;

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
            catch
            {
            }

            return string.Empty;
        }

        private static string ExtractPartialTextFromJson(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("partial", out JsonElement partialElement))
                    return partialElement.GetString() ?? string.Empty;
            }
            catch
            {
            }

            return string.Empty;
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                _isRecognitionEnabled = false;

                if (waveIn != null)
                {
                    waveIn.DataAvailable -= WaveIn_DataAvailable;
                    waveIn.RecordingStopped -= WaveIn_RecordingStopped;
                    waveIn.StopRecording();
                    waveIn.Dispose();
                    waveIn = null;
                }

                recognizer?.Dispose();
                recognizer = null;

                voskModel?.Dispose();
                voskModel = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
