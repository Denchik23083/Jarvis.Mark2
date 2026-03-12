namespace Jarvis.Mark2
{
    public partial class Form1 : Form
    {
        private FlowLayoutPanel chatPanel;

        public Form1()
        {
            InitializeComponent();

            chatPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20)
            };

            this.Controls.Add(chatPanel);

            // Чтобы работал скролл колесиком
            chatPanel.MouseEnter += (s, e) => chatPanel.Focus();

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
            AddLine("Jarvis: система запущена");
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
        }

        private void AddLine(string text)
        {
            var lbl = new Label
            {
                Text = text,
                ForeColor = text.StartsWith("Jarvis") ? Color.Cyan : Color.White,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 20, 20, 20),

                // Используем ClientSize.Width вместо Width, чтобы не учитывать рамки
                // И даем запас в 60 пикселей на полосу прокрутки
                MaximumSize = new Size(chatPanel.ClientSize.Width - 50, 0)
            };

            chatPanel.Controls.Add(lbl);

            // Подписываемся на изменение размера панели, чтобы текст подстраивался
            chatPanel.SizeChanged += (s, e) => {
                lbl.MaximumSize = new Size(chatPanel.ClientSize.Width - 50, 0);
            };

            // Исправленный скролл вниз
            chatPanel.ScrollControlIntoView(lbl);
        }
    }
}
