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
        public float Speed { get; private set; }
        public GameState State { get; private set; }

        private const float StartSpeed = 3f;   // Başlangıç hızı
        private const float MaxSpeed = 7f;     // Hız en fazla bu kadar artabilir
        private const int PipeSpacing = 220;   // Borular arası mesafe
        private const int PipeCount = 2;       // Ekrandaki boru sayısı

        public Game(int width, int height)
        {
            Width = width;
            Height = height;
            Bird = new Bird(80, PlayHeight / 2f);
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
            if (State != GameState.Playing)
                return; // Oynanmıyorsa hiçbir şey hareket etmesin

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
                    State = GameState.GameOver;
            }

            // Zemine çarptı mı?
            if (Bird.Y + Bird.Size >= PlayHeight)
                State = GameState.GameOver;
        }

        // Her 5 puanda bir oyunu biraz hızlandır
        private void IncreaseSpeed()
        {
            if (Score % 5 == 0 && Speed < MaxSpeed)
                Speed += 0.5f;
        }

        // Bütün oyunu çiz
        public void Draw(Graphics g)
        {
            foreach (Pipe pipe in Pipes)
                pipe.Draw(g);

            DrawGround(g);
            Bird.Draw(g);

            // Skor (en üstte, oyun sırasında)
            if (State == GameState.Playing)
                DrawCenteredText(g, Score.ToString(), 36, 30);

            if (State == GameState.Ready)
            {
                DrawCenteredText(g, "FLAPPY BIRD", 32, 120);
                DrawCenteredText(g, "Başlamak için SPACE", 16, 330);
            }

            if (State == GameState.GameOver)
            {
                DrawCenteredText(g, "GAME OVER", 34, 150);
                DrawCenteredText(g, $"Skor: {Score}", 22, 220);
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
        private void DrawCenteredText(Graphics g, string text, float size, float y)
        {
            using Font font = new Font("Segoe UI", size, FontStyle.Bold);
            using StringFormat format = new StringFormat { Alignment = StringAlignment.Center };

            RectangleF area = new RectangleF(0, y, Width, size * 2);
            RectangleF shadow = new RectangleF(2, y + 2, Width, size * 2);

            g.DrawString(text, font, Brushes.Black, shadow, format);  // Gölge
            g.DrawString(text, font, Brushes.White, area, format);    // Asıl yazı
        }
    }
}