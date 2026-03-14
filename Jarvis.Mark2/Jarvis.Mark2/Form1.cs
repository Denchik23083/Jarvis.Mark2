using NAudio.Wave;
using System.Text.Json;
using Vosk;

namespace Jarvis.Mark2
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel? chatPanel;

        private Model? voskModel;
        private VoskRecognizer? recognizer;
        private WaveInEvent? waveIn;

        private bool isActivated = false;

        public Form1()
        {
            InitializeComponent();
            AddPanel();
            SwitchToMainMode();
            StartVoiceRecognition();
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

        private void StartVoiceRecognition()
        {
            try
            {
                Vosk.Vosk.SetLogLevel(0);

                var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model-ru");

                if (!Directory.Exists(modelPath))
                {
                    BeginInvoke(new Action(() =>
                    {
                        AddLine($"Папка модели не найдена: {modelPath}");
                    }));

                    return;
                }

                voskModel = new Model(modelPath);
                recognizer = new VoskRecognizer(voskModel, 16000.0f);

                waveIn = new WaveInEvent()
                {
                    DeviceNumber = 0,
                    WaveFormat = new(16000, 1),
                    BufferMilliseconds = 500
                };

                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.RecordingStopped += WaveIn_RecordingStopped;

                waveIn.StartRecording();

                AddLine("Jarvis: Микрофон активирован.");
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    AddLine("Ошибка запуска распознавания: " + ex.Message);
                }));
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
                        BeginInvoke(new Action(() =>
                        {
                            ProcessRecognizedText(text);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    AddLine("Ошибка распознавания: " + ex.Message);
                }));
            }
        }

        private void WaveIn_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                BeginInvoke(new Action(() =>
                {
                    AddLine("Ошибка микрофона: " + e.Exception.Message);
                }));
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

        private void ProcessRecognizedText(string text)
        {
            text = text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(text))
                return;

            AddLine("User: " + text);


            if (!isActivated)
            {
                // Режим картинки: ждём только команду активации
                if (text == "джарвис" || text.Contains("джарвис не спишь"))
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
            waveIn?.StopRecording();
            waveIn?.Dispose();
            recognizer?.Dispose();
            voskModel?.Dispose();

            base.OnFormClosing(e);
        }
    }
}
