using Jarvis.Mark2.Infrastructure;
using System.Text.Json;

namespace Jarvis.Mark2
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel? chatPanel;
        private readonly VoiceRecognitionService voiceRecognitionService = new();

        private bool isActivated = false;

        public Form1()
        {
            InitializeComponent();
            AddPanel();
            SwitchToMainMode();
            
            voiceRecognitionService.TextRecognized += VoiceRecognitionService_TextRecognized;
            voiceRecognitionService.ErrorOccurred += VoiceRecognitionService_ErrorOccurred;
            
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

        private void ProcessRecognizedText(string text)
        {
            text = text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(text))
                return;

            AddLine("User: " + text);


            if (!isActivated)
            {
                // Режим картинки: ждём только команду активации
                if (text == "джарвис" || text == "привет" || text == "джарвис не спишь")
                {
                    isActivated = true;
                    SwitchToChatMode();
                    AddLine("Jarvis: Всегда к вашим услугам Сэр");
                }

                return;
            }

            // Режим чата
            if (text.Contains("спать") || text.Contains("спящий режим"))
            {
                isActivated = false;
                SwitchToMainMode();
                AddLine("Jarvis: До свидания сэр");
                return;
            }

            // Здесь потом будут обычные команды
            if (text.Contains("привет"))
            {
                AddLine("Jarvis: Здравствуйте.");
                return;
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
            voiceRecognitionService.OnFormClosing();

            base.OnFormClosing(e);
        }
    }
}
