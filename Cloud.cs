namespace YMH219_FlappyBird
{
    // Arka planda yavaşça akan tek bir bulut
    public class Cloud
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        private readonly float speed;  // Akış hızı (borulardan yavaş)
        private readonly float scale;  // Boyut çarpanı (1 = normal)

        public Cloud(float x, float y, float speed, float scale)
        {
            X = x;
            Y = y;
            this.speed = speed;
            this.scale = scale;
        }

        public void Update()
        {
            X -= speed;
        }

        // Bulut tamamen ekranın solundan çıktı mı?
        public bool IsOffScreen => X + 80 * scale < 0;

        // Bulutu sağdan yeniden başlat
        public void Reset(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Bulutu çiz: gündüz beyaz, gece gri-mavi
        public void Draw(Graphics g, float brightness)
        {
            int c = (int)(110 + 145 * brightness);
            using SolidBrush brush = new SolidBrush(Color.FromArgb(c, c, Math.Min(255, c + 20)));

            float s = scale;
            // Üç daire üst üste gelince bulut şekli oluşuyor
            g.FillEllipse(brush, X, Y + 10 * s, 40 * s, 22 * s);
            g.FillEllipse(brush, X + 18 * s, Y, 36 * s, 30 * s);
            g.FillEllipse(brush, X + 42 * s, Y + 8 * s, 38 * s, 24 * s);
        }
    }
}