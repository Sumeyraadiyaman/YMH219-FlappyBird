namespace YMH219_FlappyBird
{
    // Bir engel çifti: üstte bir boru, altta bir boru, ortada geçiş boşluğu
    public class Pipe
    {
        public float X { get; private set; }   // Borunun yatay konumu
        public int Width { get; } = 60;         // Boru genişliği
        public int GapSize { get; } = 160;      // Kuşun geçeceği boşluğun yüksekliği
        public int GapY { get; private set; }   // Boşluğun başladığı yükseklik

        private readonly int screenHeight;

        // Bütün borular aynı rastgele sayı üreticisini kullansın diye 'static'
        private static readonly Random random = new Random();

        public Pipe(float x, int screenHeight)
        {
            X = x;
            this.screenHeight = screenHeight;
            RandomizeGap();
        }

        // Boruyu sola kaydır
        public void Update(float speed)
        {
            X -= speed;
        }

        // Boru tamamen ekranın solundan çıktı mı?
        public bool IsOffScreen => X + Width < 0;

        // Ekrandan çıkan boruyu sağa geri taşı ve yeni bir boşluk belirle
        public void Reset(float newX)
        {
            X = newX;
            RandomizeGap();
        }

        // Boşluğun yerini rastgele seç (çok üste veya çok alta yapışmasın)
        private void RandomizeGap()
        {
            GapY = random.Next(80, screenHeight - GapSize - 80);
        }

        // Üst ve alt borunun kapladığı alanlar
        public RectangleF TopRect => new RectangleF(X, 0, Width, GapY);
        public RectangleF BottomRect => new RectangleF(X, GapY + GapSize, Width, screenHeight - (GapY + GapSize));

        // Kuş bu borulardan birine değiyor mu?
        public bool CollidesWith(Bird bird)
        {
            return bird.Bounds.IntersectsWith(TopRect) || bird.Bounds.IntersectsWith(BottomRect);
        }

        // Boruları çiz
        public void Draw(Graphics g)
        {
            using SolidBrush body = new SolidBrush(Color.FromArgb(115, 191, 46));
            using SolidBrush cap = new SolidBrush(Color.FromArgb(90, 160, 30));
            using Pen border = new Pen(Color.FromArgb(60, 110, 20), 2);

            // Gövdeler
            g.FillRectangle(body, TopRect);
            g.FillRectangle(body, BottomRect);
            g.DrawRectangle(border, TopRect.X, TopRect.Y, TopRect.Width, TopRect.Height);
            g.DrawRectangle(border, BottomRect.X, BottomRect.Y, BottomRect.Width, BottomRect.Height);

            // Boru ağızları (biraz daha geniş kapaklar)
            RectangleF topCap = new RectangleF(X - 5, GapY - 20, Width + 10, 20);
            RectangleF bottomCap = new RectangleF(X - 5, GapY + GapSize, Width + 10, 20);
            g.FillRectangle(cap, topCap);
            g.FillRectangle(cap, bottomCap);
            g.DrawRectangle(border, topCap.X, topCap.Y, topCap.Width, topCap.Height);
            g.DrawRectangle(border, bottomCap.X, bottomCap.Y, bottomCap.Width, bottomCap.Height);
        }
    }
}