namespace YMH219_FlappyBird
{
    // Oyunun olabileceği durumlar
    public enum GameState
    {
        Ready,      // Başlamayı bekliyor
        Playing,    // Oynanıyor
        GameOver    // Oyun bitti
    }

    // Oyunun bütün kurallarını ve durumunu yöneten sınıf
    public class Game
    {
        public int Width { get; }
        public int Height { get; }
        public int GroundHeight { get; } = 60;
        public int PlayHeight => Height - GroundHeight;  // Zemin hariç oynanabilir alan

        public Bird Bird { get; private set; }
        public List<Pipe> Pipes { get; } = new List<Pipe>();
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public bool IsNewRecord { get; private set; }
        public float Speed { get; private set; }
        public GameState State { get; private set; }

        public Sky Sky { get; }

        private const float StartSpeed = 3f;   // Başlangıç hızı
        private const float MaxSpeed = 7f;     // Hız en fazla bu kadar artabilir
        private const int PipeSpacing = 220;   // Borular arası mesafe
        private const int PipeCount = 2;       // Ekrandaki boru sayısı

        // Rekorun saklanacağı dosya (uygulamanın çalıştığı klasörde)
        private readonly string highScoreFile = Path.Combine(Application.StartupPath, "highscore.txt");

        public Game(int width, int height)
        {
            Width = width;
            Height = height;
            Bird = new Bird(80, PlayHeight / 2f);
            Sky = new Sky(Width, PlayHeight);
            LoadHighScore();
            Reset();
        }

        // Yeni oyun: her şeyi başlangıç haline getir
        public void Reset()
        {
            Bird = new Bird(80, PlayHeight / 2f);

            Pipes.Clear();
            for (int i = 0; i < PipeCount; i++)
            {
                Pipes.Add(new Pipe(Width + 100 + i * PipeSpacing, PlayHeight));
            }

            Score = 0;
            Speed = StartSpeed;
            IsNewRecord = false;
            State = GameState.Ready;
        }

        // Oyuncu zıplama tuşuna bastığında
        public void Flap()
        {
            if (State == GameState.Ready)
                State = GameState.Playing;   // İlk basış oyunu başlatır

            if (State == GameState.Playing)
                Bird.Jump();
        }

        // Her oyun adımında çağrılır
        public void Update()
        {
            Sky.Update(); // Gökyüzü her durumda hareket etsin

            // Bekleme ekranında kuş yerinde süzülsün
            if (State == GameState.Ready)
            {
                Bird.Hover();
                return;
            }

            if (State != GameState.Playing)
                return; // Oyun bittiyse hiçbir şey hareket etmesin

            Bird.Update();

            foreach (Pipe pipe in Pipes)
            {
                pipe.Update(Speed);

                // Kuş boruyu geçti mi? (borunun sağ kenarı kuşun soluna geçtiyse)
                if (!pipe.Passed && pipe.X + pipe.Width < Bird.X)
                {
                    pipe.Passed = true;
                    Score++;
                    IncreaseSpeed();
                }

                // Ekrandan çıkan boruyu en sona taşı
                if (pipe.IsOffScreen)
                    pipe.Reset(pipe.X + Pipes.Count * PipeSpacing);

                // Boruya çarptı mı?
                if (pipe.CollidesWith(Bird))
                    EndGame();
            }

            // Zemine çarptı mı?
            if (Bird.Y + Bird.Size >= PlayHeight)
                EndGame();
        }

        // Oyunu bitir ve gerekirse rekoru kaydet
        private void EndGame()
        {
            if (State == GameState.GameOver)
                return; // Zaten bittiyse tekrar işlem yapma

            State = GameState.GameOver;

            if (Score > HighScore)
            {
                HighScore = Score;
                IsNewRecord = true;
                SaveHighScore();
            }
        }

        // Her 5 puanda bir oyunu biraz hızlandır
        private void IncreaseSpeed()
        {
            if (Score % 5 == 0 && Speed < MaxSpeed)
                Speed += 0.5f;
        }

        // Rekoru dosyadan oku
        private void LoadHighScore()
        {
            try
            {
                if (File.Exists(highScoreFile))
                {
                    string text = File.ReadAllText(highScoreFile);
                    if (int.TryParse(text, out int value))
                        HighScore = value;
                }
            }
            catch
            {
                HighScore = 0; // Dosya okunamazsa rekoru 0 kabul et, oyun yine çalışsın
            }
        }

        // Rekoru dosyaya yaz
        private void SaveHighScore()
        {
            try
            {
                File.WriteAllText(highScoreFile, HighScore.ToString());
            }
            catch
            {
                // Kaydedilemezse oyunu durdurmaya değmez; sessizce devam et
            }
        }

        // Bütün oyunu çiz
        public void Draw(Graphics g)
        {
            Sky.Draw(g);  // En arkada gökyüzü

            foreach (Pipe pipe in Pipes)
                pipe.Draw(g);

            DrawGround(g);
            Bird.Draw(g);
            Sky.DrawNightOverlay(g, Height);  // Gece karartması (yazılardan önce)

            // Skor (en üstte, oyun sırasında)
            if (State == GameState.Playing)
                DrawCenteredText(g, Score.ToString(), 36, 30);

            if (State == GameState.Ready)
            {
                DrawCenteredText(g, "FLAPPY BIRD", 32, 120);
                DrawCenteredText(g, "Başlamak için SPACE", 16, 330);
                DrawCenteredText(g, $"En yüksek skor: {HighScore}", 14, 380);
            }

            if (State == GameState.GameOver)
            {
                DrawCenteredText(g, "GAME OVER", 34, 150);
                DrawCenteredText(g, $"Skor: {Score}", 22, 220);

                if (IsNewRecord)
                    DrawCenteredText(g, "YENİ REKOR!", 18, 265, Brushes.Gold);
                else
                    DrawCenteredText(g, $"En yüksek: {HighScore}", 16, 268);

                DrawCenteredText(g, "Yeniden başlamak için R", 16, 330);
            }
        }

        private void DrawGround(Graphics g)
        {
            using SolidBrush dirt = new SolidBrush(Color.FromArgb(222, 184, 135));
            using SolidBrush grass = new SolidBrush(Color.FromArgb(100, 180, 60));

            g.FillRectangle(dirt, 0, PlayHeight, Width, GroundHeight);
            g.FillRectangle(grass, 0, PlayHeight, Width, 12);
        }

        // Yazıyı yatayda ortalayarak, gölgeli şekilde yazar
        private void DrawCenteredText(Graphics g, string text, float size, float y, Brush? color = null)
        {
            using Font font = new Font("Segoe UI", size, FontStyle.Bold);
            using StringFormat format = new StringFormat { Alignment = StringAlignment.Center };

            RectangleF area = new RectangleF(0, y, Width, size * 2);
            RectangleF shadow = new RectangleF(2, y + 2, Width, size * 2);

            g.DrawString(text, font, Brushes.Black, shadow, format);           // Gölge
            g.DrawString(text, font, color ?? Brushes.White, area, format);   // Asıl yazı
        }
    }
}