using System.Media;
using Windows.Media.SpeechSynthesis;

namespace Jarvis.Mark2.Infrastructure.Services
{
    public class TtsService : IDisposable
    {
        private readonly SpeechSynthesizer _synth = new();
        private SoundPlayer? _player;
        private string? _currentFile;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task SpeakAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            await _semaphore.WaitAsync();
            try
            {
                StopInternal();

                using var speechStream = await _synth.SynthesizeTextToStreamAsync(text);

                string tempFile = Path.Combine(
                    Path.GetTempPath(),
                    $"jarvis_tts_{Guid.NewGuid():N}.wav");

                await using (var input = speechStream.AsStreamForRead())
                await using (var output = File.Create(tempFile))
                {
                    await input.CopyToAsync(output);
                }

                _currentFile = tempFile;
                _player = new SoundPlayer(tempFile);
                _player.Load();
                _player.Play();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Stop()
        {
            _semaphore.Wait();
            try
            {
                StopInternal();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void StopInternal()
        {
            _player?.Stop();
            _player?.Dispose();
            _player = null;

            if (!string.IsNullOrWhiteSpace(_currentFile) && File.Exists(_currentFile))
            {
                try
                {
                    File.Delete(_currentFile);
                }
                catch
                {
                }
            }

            _currentFile = null;
        }

        public void Dispose()
        {
            Stop();
            _synth.Dispose();
            _semaphore.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}