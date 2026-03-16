using Jarvis.Mark2.Infrastructure;
using System.Text.Json;

namespace Jarvis.Mark2
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel? chatPanel;
        private Label? partialLabel;
        private readonly VoiceRecognitionService voiceRecognitionService = new();
        private readonly CommandParser commandParser = new();

        private bool isActivated = false;

        public Form1()
        {
            InitializeComponent();
            AddPanel();
            SwitchToMainMode();
            
            voiceRecognitionService.TextRecognized += VoiceRecognitionService_TextRecognized;
            voiceRecognitionService.ErrorOccurred += VoiceRecognitionService_ErrorOccurred;
            voiceRecognitionService.PartialTextRecognized += OnPartialText;

            Shown += Form1_Shown;
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            voiceRecognitionService.StartVoiceRecognition();
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

            // Исправленный скролл вниз
            chatPanel.ScrollControlIntoView(lbl);
        }

        private void VoiceRecognitionService_TextRecognized(string text)
        {
            //if (!IsHandleCreated) return;
            
            BeginInvoke(new Action(() =>
            {
                ProcessRecognizedText(text);
            }));
        }

        private void VoiceRecognitionService_ErrorOccurred(string message)
        {
            BeginInvoke(new Action(() =>
            {
                AddLine("Jarvis: " + message);
            }));
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

        private void ProcessRecognizedText(string text)
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
                    AddLine("Jarvis: Всегда к вашим услугам Сэр");
                    break;

                case CommandType.Sleep:
                    isActivated = false;
                    AddLine("Jarvis: До свидания сэр");
                    SwitchToMainMode();
                    break;

                case CommandType.System:
                    ExecuteSystemCommand(result.SystemCommandType);
                    break;

                case CommandType.AiQuery:
                    //Ai
                    break;

                case CommandType.None:
                default:
                    AddLine("Jarvis: Повторите пожалуйста.");
                    break;
            }
        }

        private void ExecuteSystemCommand(SystemCommandType systemCommandType)
        {
            switch (systemCommandType)
            {
                case SystemCommandType.OpenGoogle:
                    AddLine("Jarvis: Открываю Google.");
                    break;

                case SystemCommandType.OpenYouTube:
                    AddLine("Jarvis: Открываю YouTube.");
                    break;

                case SystemCommandType.Mute:
                    AddLine("Jarvis: Перехожу в тихий режим.");
                    break;

                case SystemCommandType.UnMute:
                    AddLine("Jarvis: Звук возвращён.");
                    break;


                case SystemCommandType.None:
                default:
                    break;
            }
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

            base.OnFormClosing(e);
        }
    }
}
