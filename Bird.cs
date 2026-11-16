namespace YMH219_FlappyBird
{
    // Oyuncunun kontrol ettiği kuş
    public class Bird
    {
        public float X { get; }               // Yatay konum (sabit, kuş ileri gitmez)
        public float Y { get; private set; }  // Dikey konum (sadece kuş kendisi değiştirir)
        public int Size { get; } = 30;        // Kuşun çapı (piksel)

        private float velocity = 0;               // Düşme hızı
        private const float Gravity = 0.5f;       // Her adımda hıza eklenen yer çekimi
        private const float JumpStrength = -8f;   // Zıplayınca verilen hız (eksi = yukarı)

        public Bird(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Zıpla: hızı yukarı doğru ayarla
        public void Jump()
        {
            velocity = JumpStrength;
        }

        // Her oyun adımında çağrılır: yer çekimi hızı artırır, hız konumu değiştirir
        public void Update()
        {
            velocity += Gravity;
            Y += velocity;
        }

        // Kuşun kapladığı alan (çarpışma kontrolünde kullanacağız)
        public RectangleF Bounds => new RectangleF(X, Y, Size, Size);

        // Kuşu ekrana çizer
        public void Draw(Graphics g)
        {
            // Gövde
            g.FillEllipse(Brushes.Gold, Bounds);
            g.DrawEllipse(Pens.DarkOrange, X, Y, Size, Size);

            // Göz
            g.FillEllipse(Brushes.White, X + 17, Y + 6, 9, 9);
            g.FillEllipse(Brushes.Black, X + 21, Y + 9, 4, 4);

            // Gaga (üçgen)
            PointF[] beak =
            {
                new PointF(X + Size - 2, Y + 13),
                new PointF(X + Size + 8, Y + 17),
                new PointF(X + Size - 2, Y + 21)
            };
            g.FillPolygon(Brushes.OrangeRed, beak);
        }
    }
}