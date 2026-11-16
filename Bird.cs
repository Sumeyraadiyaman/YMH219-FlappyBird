using System.Drawing.Drawing2D;

namespace YMH219_FlappyBird
{
    // Oyuncunun kontrol ettiği kuş
    public class Bird
    {
        public float X { get; }               // Yatay konum (sabit, kuş ileri gitmez)
        public float Y { get; private set; }  // Dikey konum (sadece kuş kendisi değiştirir)
        public int Size { get; } = 30;        // Çarpışma kutusunun boyutu (piksel)

        private float velocity = 0;               // Düşme hızı
        private const float Gravity = 0.5f;       // Her adımda hıza eklenen yer çekimi
        private const float JumpStrength = -8f;   // Zıplayınca verilen hız (eksi = yukarı)

        // Gövde ölçüleri: enine biraz daha geniş, yatık bir oval
        private const float BodyWidth = 36f;
        private const float BodyHeight = 28f;

        private float animationTime = 0;  // Kanat ve süzülme animasyonu için sayaç
        private readonly float startY;    // Bekleme ekranındaki süzülmenin merkezi

        public Bird(float x, float y)
        {
            X = x;
            Y = y;
            startY = y;
        }

        // Zıpla: hızı yukarı doğru ayarla
        public void Jump()
        {
            velocity = JumpStrength;
        }

        // Oyun sırasında her adımda: yer çekimi + hareket + animasyon
        public void Update()
        {
            velocity += Gravity;
            Y += velocity;
            animationTime++;
        }

        // Bekleme ekranında: yerinde hafifçe aşağı yukarı süzül
        public void Hover()
        {
            animationTime++;
            Y = startY + MathF.Sin(animationTime * 0.1f) * 8;
        }

        // Kanadın açısı: sinüs dalgası ile -35° ile +35° arasında gidip gelir
        private float WingAngle => MathF.Sin(animationTime * 0.6f) * 35;

        // Gövdenin eğimi: yükselirken yukarı, düşerken aşağı bakar
        private float BodyAngle => Math.Clamp(velocity * 4, -25f, 70f);

        // Kuşun kapladığı alan (çarpışma kontrolünde kullanılır)
        public RectangleF Bounds => new RectangleF(X, Y, Size, Size);

        // Kuşu ekrana çizer
        public void Draw(Graphics g)
        {
            var savedState = g.Save();
            g.TranslateTransform(X + Size / 2f, Y + Size / 2f);  // Tuvalin merkezini kuşun ortasına taşı
            g.RotateTransform(BodyAngle);                          // Kuşun eğimi kadar döndür

            using Pen softOutline = new Pen(Color.FromArgb(110, 220, 120, 160), 1.2f); // İnce, yarı saydam kenar

            DrawTail(g, softOutline);
            DrawBody(g, softOutline);
            DrawFace(g);
            DrawWing(g, softOutline);

            g.Restore(savedState);  // Tuvali eski haline getir
        }

        // Kuyruk: gövdeden tek parça çıkıp ucuna doğru incelen yumuşak bir tüy
        private void DrawTail(Graphics g, Pen outline)
        {
            PointF[] tailPoints =
            {
                new PointF(-15, -4),   // Gövdeye bağlandığı üst nokta
                new PointF(-23, -9),   // Yukarı kıvrılan kısım
                new PointF(-27, -7),   // Uç
                new PointF(-22, -1),   // Alttan geri dönüş
                new PointF(-15, 4)     // Gövdeye bağlandığı alt nokta
            };

            using GraphicsPath tailPath = new GraphicsPath();
            tailPath.AddClosedCurve(tailPoints, 0.5f); // Noktaları yumuşak bir eğriyle birleştir

            using SolidBrush tailBrush = new SolidBrush(Color.FromArgb(255, 190, 210));
            g.FillPath(tailBrush, tailPath);
            g.DrawPath(outline, tailPath);
        }

        // Gövde: sol üstten gelen ışıkla beyazdan pembeye yumuşak geçiş
        private void DrawBody(Graphics g, Pen outline)
        {
            using GraphicsPath bodyPath = new GraphicsPath();
            bodyPath.AddEllipse(-BodyWidth / 2, -BodyHeight / 2, BodyWidth, BodyHeight);

            using PathGradientBrush bodyBrush = new PathGradientBrush(bodyPath);
            bodyBrush.CenterPoint = new PointF(-5, -6);               // Işığın en parlak noktası
            bodyBrush.CenterColor = Color.White;                      // Merkez: bembeyaz
            bodyBrush.SurroundColors = new[] { Color.FromArgb(255, 214, 228) }; // Kenarlar: pembe

            g.FillPath(bodyBrush, bodyPath);
            g.DrawPath(outline, bodyPath);

            // Başın üstündeki iki küçük tüy
            using SolidBrush tuftBrush = new SolidBrush(Color.FromArgb(255, 170, 200));
            g.FillEllipse(tuftBrush, -1, -19, 4, 8);
            g.FillEllipse(tuftBrush, 2, -17, 4, 6);
        }

        // Yüz: göz, yanak, gaga
        private void DrawFace(Graphics g)
        {
            // Göz: koyu, oval, iki parlama noktalı
            using SolidBrush eyeBrush = new SolidBrush(Color.FromArgb(50, 35, 45));
            g.FillEllipse(eyeBrush, 5, -8, 7, 9);
            g.FillEllipse(Brushes.White, 7.5f, -6.5f, 3, 3);       // Büyük parlama
            g.FillEllipse(Brushes.White, 6.5f, -2.5f, 1.5f, 1.5f); // Küçük parlama

            // Yanak: yarı saydam pembe
            using SolidBrush blushBrush = new SolidBrush(Color.FromArgb(110, 255, 120, 160));
            g.FillEllipse(blushBrush, 3, 2, 9, 5);

            // Gaga: gövdenin ön kenarından çıkan üst ve alt parça
            using SolidBrush beakTop = new SolidBrush(Color.FromArgb(255, 170, 120));
            using SolidBrush beakBottom = new SolidBrush(Color.FromArgb(240, 130, 95));

            PointF[] upper = { new PointF(16, -2), new PointF(24, 1), new PointF(16, 2) };
            PointF[] lower = { new PointF(16, 2), new PointF(22, 2), new PointF(16, 5) };
            g.FillPolygon(beakTop, upper);
            g.FillPolygon(beakBottom, lower);
        }

        // Kanat: omuz etrafında dönen, üstten alta renk geçişli, tüy çizgili
        private void DrawWing(Graphics g, Pen outline)
        {
            var wingState = g.Save();
            g.TranslateTransform(-5, 3);      // Omuz noktasına git
            g.RotateTransform(WingAngle);     // Kanat açısı kadar döndür

            RectangleF wingRect = new RectangleF(-12, -4, 15, 9);
            using LinearGradientBrush wingBrush = new LinearGradientBrush(
                wingRect,
                Color.FromArgb(255, 225, 236),   // Üst: açık pembe
                Color.FromArgb(245, 160, 192),   // Alt: koyu pembe
                LinearGradientMode.Vertical);

            g.FillEllipse(wingBrush, wingRect);
            g.DrawEllipse(outline, wingRect);

            // Tüy çizgileri
            using Pen featherPen = new Pen(Color.FromArgb(120, 220, 110, 150), 1f);
            g.DrawLine(featherPen, -9, 1, -3, 1);
            g.DrawLine(featherPen, -8, 3, -4, 3);

            g.Restore(wingState);
        }
    }
}