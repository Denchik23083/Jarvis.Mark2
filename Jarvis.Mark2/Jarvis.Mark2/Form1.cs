namespace Jarvis.Mark2
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel chatPanel = null!;

        public Form1()
        {
            InitializeComponent();
            AddPanel();
            SwitchToMainMode();
        }

        private void AddPanel()
        {
            chatPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20),
                Visible = false,
                Size = new Size(2500, 450)
            };

            Controls.Add(chatPanel);
            chatPanel.SendToBack();

            // Приветственные строки
            AddLine("Jarvis: Система готова. Графический модуль отключен.");
            AddLine("User: Принято.");

            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");

            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");
            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: слушаю");

            AddLine("Jarvis: система запущена");
            AddLine("User: привет");
            AddLine("Jarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущенаJarvis: система запущена");

        }

        private void AddLine(string text)
        {
            if (chatPanel == null) return;

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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (chatPanel == null) return;

            foreach (Control control in chatPanel.Controls)
            {
                if (control is Label lbl)
                {
                    lbl.MaximumSize = new Size(chatPanel.ClientSize.Width - chatPanel.Padding.Left - chatPanel.Padding.Right - 20, 0);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SwitchToChatMode();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SwitchToMainMode();
        }       

        private void SwitchToMainMode()
        {
            pictureBox1.Visible = true;
            chatPanel.Visible = false;

            pictureBox1.SendToBack();

            button1.BringToFront();
            button2.BringToFront();
        }

        private void SwitchToChatMode()
        {
            pictureBox1.Visible = false;
            chatPanel.Visible = true;

            chatPanel.BringToFront();

            button1.BringToFront();
            button2.BringToFront();
        }
    }
}
