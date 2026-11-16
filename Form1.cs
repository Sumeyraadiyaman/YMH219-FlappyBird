namespace YMH219_FlappyBird
{
    public partial class Form1 : Form
    {
        // Oyun döngüsü: belirli aralıklarla Tick olayını tetikler
        private readonly System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        private Bird bird;
        private readonly List<Pipe> pipes = new List<Pipe>();

        private const float PipeSpeed = 3f;     // Boruların sola kayma hızı
        private const int PipeSpacing = 220;    // İki boru arasındaki yatay mesafe

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

            bird = new Bird(80, 250);

            // İki boru oluştur: biri ekranın hemen sağında, diğeri onun arkasında
            pipes.Add(new Pipe(ClientSize.Width, ClientSize.Height));
            pipes.Add(new Pipe(ClientSize.Width + PipeSpacing, ClientSize.Height));

            // Olayları bağla
            gameTimer.Interval = 20; // 20 milisaniyede bir (saniyede ~50 kare)
            gameTimer.Tick += GameTimer_Tick;
            KeyDown += Form1_KeyDown;
            Paint += Form1_Paint;

            gameTimer.Start();
        }

        // Her 20 ms'de bir çalışır: oyunu bir adım ilerletir
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            bird.Update();

            foreach (Pipe pipe in pipes)
            {
                pipe.Update(PipeSpeed);

                // Ekrandan çıkan boruyu en sona taşı
                if (pipe.IsOffScreen)
                    pipe.Reset(pipe.X + pipes.Count * PipeSpacing);
            }

            // GEÇİCİ: Kuş ekrandan düşerse başa al (Game Over'ı sonra ekleyeceğiz)
            if (bird.Y > ClientSize.Height)
                bird = new Bird(80, 250);

            Invalidate(); // Ekranı yeniden çizdir
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Up)
                bird.Jump();
        }

        // Ekran her yeniden çizildiğinde çalışır
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // Kenarları yumuşat

            foreach (Pipe pipe in pipes)
                pipe.Draw(e.Graphics);

            bird.Draw(e.Graphics); // Kuş boruların önünde görünsün diye en son çizilir
        }
    }
}