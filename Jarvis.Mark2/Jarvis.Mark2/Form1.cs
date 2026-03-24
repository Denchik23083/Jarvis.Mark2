using Jarvis.Mark2.Infrastructure.Core;
using Jarvis.Mark2.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Jarvis.Mark2
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel? chatPanel;
        private Label? partialLabel;
        private Label? jarvisPartialLabel;
        private readonly VoiceRecognitionService voiceRecognitionService = new();
        private readonly CommandParser commandParser = new();
        private readonly TtsService Jarvis = new();
        private readonly GeminiService geminiService;

        private readonly HashSet<string> listAgree = ["да сэр", "слушаюсь сэр", "приступаю сэр", "выполняю сэр", "как пожелаете сэр"];
        private readonly Random random = new();

        private bool isActivated = false;
        private bool isAiBusy = false;
        private bool isMuted = false;

        public Form1()
        {
            InitializeComponent();
            AddPanel();
            SwitchToMainMode();

            string apiKey = GetApiKey();
            geminiService = new GeminiService(apiKey);

            voiceRecognitionService.TextRecognized += VoiceRecognitionService_TextRecognized;
            voiceRecognitionService.ErrorOccurred += VoiceRecognitionService_ErrorOccurred;
            voiceRecognitionService.PartialTextRecognized += OnPartialText;

            Shown += Form1_Shown;
        }

        private static string GetApiKey()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return config["Gemini:ApiKey"] ?? throw new Exception("API key не найден");
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            voiceRecognitionService.StartVoiceRecognition();
        }

        private void VoiceRecognitionService_TextRecognized(string text)
        {
            BeginInvoke(new Action(() =>
            {
                _ = ProcessRecognizedTextAsync(text);
            }));
        }

        private void VoiceRecognitionService_ErrorOccurred(string message)
        {
            BeginInvoke(new Action(() => AddLine("Jarvis: " + message)));
        }

        private void OnPartialText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnPartialText(text)));
                return;
            }

            if (chatPanel is null) return;

            if (partialLabel is null)
            {
                partialLabel = new Label
                {
                    ForeColor = text.StartsWith("Jarvis") ? Color.Cyan : Color.White,
                    Font = new Font("Consolas", 14, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 50),
                    MaximumSize = new Size(chatPanel.ClientSize.Width - chatPanel.Padding.Left - chatPanel.Padding.Right - 20, 0)
                };

                chatPanel.Controls.Add(partialLabel);
            }

            partialLabel.Text = "User: " + text;
            chatPanel.ScrollControlIntoView(partialLabel);
        }

        private async Task ProcessRecognizedTextAsync(string text)
        {
            if (partialLabel is not null)
            {
                chatPanel?.Controls.Remove(partialLabel);
                partialLabel.Dispose();
                partialLabel = null;
            }

            text = text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(text))
                return;

            AddLine("User: " + text);

            var result = commandParser.Parse(text, isActivated);

            switch (result.CommandType)
            {
                case CommandType.Wake:
                    isActivated = true;
                    SwitchToChatMode();
                    await SpeakAndShowAsync("Всегда к вашим услугам сэр");
                    break;

                case CommandType.Sleep:
                    isActivated = false;
                    await SpeakAndShowAsync("До свидания сэр");
                    SwitchToMainMode();
                    break;

                case CommandType.System:
                    await ExecuteSystemCommandAsync(result.SystemCommandType);
                    break;

                case CommandType.AiQuery:
                    await HandleAiQueryAsync(text);
                    break;

                case CommandType.None:
                default:
                    await SpeakAndShowAsync("Повторите пожалуйста");
                    break;
            }
        }

        private async Task ExecuteSystemCommandAsync(SystemCommandType systemCommandType)
        {
            var randomAgree = listAgree.ElementAt(random.Next(listAgree.Count));

            switch (systemCommandType)
            {
                case SystemCommandType.OpenGoogle:
                    await SpeakAndShowAsync(randomAgree);
                    OpenUrl("https://google.com");
                    break;

                case SystemCommandType.OpenYouTube:
                    await SpeakAndShowAsync(randomAgree);
                    OpenUrl("https://www.youtube.com");
                    break;

                case SystemCommandType.OpenSteam:
                    await SpeakAndShowAsync(randomAgree);
                    OpenUrl(@"C:\Program Files (x86)\Steam\steam.exe");
                    break;

                case SystemCommandType.OpenWot:
                    await SpeakAndShowAsync(randomAgree);
                    OpenUrl(@"C:\ProgramData\Wargaming.net\GameCenter\wgc.exe");
                    break;
                
                case SystemCommandType.Clear:
                    await SpeakAndShowAsync(randomAgree);
                    chatPanel?.Controls.Clear();
                    break;

                case SystemCommandType.Mute:
                    MuteJarvis();
                    break;

                case SystemCommandType.UnMute:
                    UnMuteJarvis();
                    await SpeakAndShowAsync("звук возвращен");
                    break;

                case SystemCommandType.None:
                default:
                    break;
            }
        }

        private void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private async Task HandleAiQueryAsync(string text)
        {
            if (!ShouldSendToAi(text))
            {
                await SpeakAndShowAsync("Повторите пожалуйста");
                return;
            }

            if (isAiBusy)
            {
                await SpeakAndShowAsync("Подождите пожалуйста");
                return;
            }

            try
            {
                isAiBusy = true;
                string cleanedText = commandParser.CleanAiText(text);
                string answer = await geminiService.AskAsync(cleanedText);

                await SpeakAndShowAsync(answer);

                return;
            }
            catch (Exception e)
            {
                string userMessage;

                if (e.Message.Contains("quota") || e.Message.Contains("limit"))
                {
                    userMessage = "Я исчерпал лимит запросов. Попробуйте позже.";
                }
                else
                {
                    userMessage = "Произошла ошибка при обращении к серверу.";
                }

                await SpeakAndShowAsync(userMessage);
            }
            finally
            {
                isAiBusy = false;
            }
        }

        private static bool ShouldSendToAi(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim().ToLower();

            if (text.Length < 3)
                return false;

            string[] ignored =
            [
                "ага", "да", "нет", "ну", "эм", "мм", "а", "и", "чё", "че"
            ];

            if (ignored.Contains(text))
                return false;

            return true;
        }

        private async Task SpeakAndShowAsync(string text)
        {
            if (isMuted)
            {
                AddLine("Jarvis: " + text);
                return;
            }

            var speechText = NorilizedText(text);

            var speakTask = SpeakWithPauseAsync(speechText);
            var showTask = ShowJarvisSpeechAsync(text);

            await Task.WhenAll(speakTask, showTask);
        }

        private void MuteJarvis()
        {
            isMuted = true;
            Jarvis.Stop();
        }

        private void UnMuteJarvis()
        {
            isMuted = false;
        }

        private async Task SpeakWithPauseAsync(string text)
        {
            try
            {
                voiceRecognitionService.StopVoiceRecognition();

                await Jarvis.SpeakAsync(text);

                await Task.Delay(500);
            }
            finally
            {
                voiceRecognitionService.StartVoiceRecognition();
            }
        }

        private async Task ShowJarvisSpeechAsync(string text)
        {
            if (InvokeRequired)
            {
                await Invoke(new Func<Task>(() => ShowJarvisSpeechAsync(text)));
                return;
            }

            if (chatPanel is null || string.IsNullOrWhiteSpace(text))
                return;

            if (jarvisPartialLabel is not null)
            {
                chatPanel.Controls.Remove(jarvisPartialLabel);
                jarvisPartialLabel.Dispose();
                jarvisPartialLabel = null;
            }

            jarvisPartialLabel = new Label
            {
                ForeColor = Color.Cyan,
                Font = new Font("Consolas", 14, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 50),
                MaximumSize = new Size(chatPanel.ClientSize.Width - chatPanel.Padding.Left - chatPanel.Padding.Right - 20, 0)
            };

            chatPanel.Controls.Add(jarvisPartialLabel);
            chatPanel.ScrollControlIntoView(jarvisPartialLabel);

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string current = "Jarvis: ";

            foreach (var word in words)
            {
                current += word + " ";
                jarvisPartialLabel.Text = current.TrimEnd();
                chatPanel.ScrollControlIntoView(jarvisPartialLabel);

                await Task.Delay(120);
            }

            var finalText = jarvisPartialLabel.Text;

            chatPanel.Controls.Remove(jarvisPartialLabel);
            jarvisPartialLabel.Dispose();
            jarvisPartialLabel = null;

            AddLine(finalText);
        }

        private static string NorilizedText(string text)
        {
            return Regex.Replace(
                text,
                @"\bсэр\b",
                "ссэр",
                RegexOptions.IgnoreCase);
        }

        private void AddPanel()
        {
            if (chatPanel is not null) return;

            chatPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20, 20, 20, 120),
                Visible = false,
                Size = new Size(2500, 450)
            };

            Controls.Add(chatPanel);
            chatPanel.BringToFront();
        }

        private void AddLine(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddLine(text)));
                return;
            }

            if (chatPanel is null) return;

            var lbl = new Label
            {
                Text = text,
                ForeColor = text.StartsWith("Jarvis") ? Color.Cyan : Color.White,
                Font = new Font("Consolas", 14, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 50),
                MaximumSize = new Size(chatPanel.ClientSize.Width - chatPanel.Padding.Left - chatPanel.Padding.Right - 20, 0)
            };

            chatPanel.Controls.Add(lbl);
            chatPanel.ScrollControlIntoView(lbl);
        }

        private void SwitchToMainMode()
        {
            pictureBox1.Visible = true;
            
            if (chatPanel is not null) chatPanel.Visible = false;

            pictureBox1.BringToFront();
        }

        private void SwitchToChatMode()
        {
            pictureBox1.Visible = false;

            if (chatPanel is not null)
            {
                chatPanel.Visible = true;
            }

            chatPanel?.BringToFront();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (chatPanel is null) return;

            foreach (Control control in chatPanel.Controls)
            {
                if (control is Label lbl)
                {
                    lbl.MaximumSize = new Size(chatPanel.ClientSize.Width - chatPanel.Padding.Left - chatPanel.Padding.Right - 20, 0);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            voiceRecognitionService.Dispose();
            Jarvis.Dispose();
            base.OnFormClosing(e);
        }
    }
}
