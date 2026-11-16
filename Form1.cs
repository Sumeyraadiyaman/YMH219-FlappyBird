namespace YMH219_FlappyBird
{
    public partial class Form1 : Form
    {
        // Oyun döngüsü: belirli aralıklarla Tick olayını tetikler
        private readonly System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        private readonly Game game;

        public Form1()
        {
            InitializeComponent();

            // Pencere ayarları
            Text = "Flappy Bird";
            ClientSize = new Size(400, 600);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.SkyBlue;
            DoubleBuffered = true; // Titremeyi önler

            game = new Game(ClientSize.Width, ClientSize.Height);

            // Olayları bağla
            gameTimer.Interval = 20; // 20 milisaniyede bir (saniyede ~50 kare)
            gameTimer.Tick += GameTimer_Tick;
            KeyDown += Form1_KeyDown;
            MouseDown += Form1_MouseDown;
            Paint += Form1_Paint;

            gameTimer.Start();
        }

        // Her 20 ms'de bir: oyunu bir adım ilerlet ve ekranı yeniden çiz
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            game.Update();
            Invalidate();
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Up)
                game.Flap();

            // Oyun bittiyse R ile yeniden başlat
            if (e.KeyCode == Keys.R && game.State == GameState.GameOver)
                game.Reset();
        }

        // Fareyle tıklayınca da zıplasın
        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            game.Flap();
        }

        // Ekran her yeniden çizildiğinde çalışır
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // Kenarları yumuşat
            game.Draw(e.Graphics);
        }
    }
}