using System.Drawing.Drawing2D;

namespace YMH219_FlappyBird
{
    // Gökyüzü: gece-gündüz döngüsü, güneş, ay, yıldızlar ve bulutlar
    public class Sky
    {
        private readonly int width;
        private readonly int horizonY;   // Ufuk çizgisi (zeminin başladığı yer)

        private float cycle = 0f;                        // Gün içindeki konum: 0 = öğlen, 0.5 = gece yarısı
        private const float CycleSpeed = 1f / 3000f;     // 3000 adımda bir tam gün (~60 saniye)
        private int tick = 0;                            // Yıldız parıltısı için sayaç

        private readonly List<Cloud> clouds = new List<Cloud>();
        private readonly List<PointF> stars = new List<PointF>();
        private readonly List<float> starPhases = new List<float>(); // Her yıldız farklı anda parlasın

        private static readonly Random random = new Random();

        // Gökyüzü renkleri (üst ve ufuk)
        private static readonly Color DayTop = Color.FromArgb(90, 170, 235);
        private static readonly Color DayBottom = Color.FromArgb(175, 225, 250);
        private static readonly Color NightTop = Color.FromArgb(8, 12, 40);
        private static readonly Color NightBottom = Color.FromArgb(35, 40, 90);
        private static readonly Color SunsetTop = Color.FromArgb(110, 80, 160);
        private static readonly Color SunsetBottom = Color.FromArgb(255, 150, 100);

        public Sky(int width, int horizonY)
        {
            this.width = width;
            this.horizonY = horizonY;

            // Rastgele yerlerde 40 yıldız
            for (int i = 0; i < 40; i++)
            {
                stars.Add(new PointF(random.Next(0, width), random.Next(0, horizonY - 60)));
                starPhases.Add((float)(random.NextDouble() * MathF.PI * 2));
            }

            // Farklı hız ve boyutta 4 bulut
            for (int i = 0; i < 4; i++)
            {
                float speed = 0.3f + (float)random.NextDouble() * 0.4f;
                float scale = 0.7f + (float)random.NextDouble() * 0.6f;
                clouds.Add(new Cloud(random.Next(0, width), random.Next(30, horizonY / 2), speed, scale));
            }
        }

        // Aydınlık: 1 = tam gündüz, 0 = tam gece
        public float Brightness => (MathF.Cos(cycle * MathF.PI * 2) + 1) / 2;

        public void Update()
        {
            cycle += CycleSpeed;
            if (cycle >= 1)
                cycle -= 1;   // Gün bitti, yeni gün başlasın (döngü)

            tick++;

            foreach (Cloud cloud in clouds)
            {
                cloud.Update();
                if (cloud.IsOffScreen)
                    cloud.Reset(width + 20, random.Next(30, horizonY / 2));
            }
        }

        public void Draw(Graphics g)
        {
            DrawGradient(g);
            DrawStars(g);
            DrawSunAndMoon(g);

            foreach (Cloud cloud in clouds)
                cloud.Draw(g, Brightness);
        }

        // Gece olunca bütün ekranı hafifçe karart (en son, yazılardan önce çizilir)
        public void DrawNightOverlay(Graphics g, int fullHeight)
        {
            int alpha = (int)((1 - Brightness) * 90);
            if (alpha <= 0)
                return;

            using SolidBrush shade = new SolidBrush(Color.FromArgb(alpha, 10, 15, 50));
            g.FillRectangle(shade, 0, 0, width, fullHeight);
        }

        // Gökyüzü rengi: gündüz, gece ve gün batımı renklerinin karışımı
        private void DrawGradient(Graphics g)
        {
            float b = Brightness;
            float sunset = 1 - MathF.Abs(b - 0.5f) * 2;   // Yarı aydınlıkta (alacakaranlık) en yüksek

            Color top = Lerp(NightTop, DayTop, b);
            top = Lerp(top, SunsetTop, sunset * 0.6f);

            Color bottom = Lerp(NightBottom, DayBottom, b);
            bottom = Lerp(bottom, SunsetBottom, sunset * 0.8f);

            Rectangle area = new Rectangle(0, 0, width, horizonY + 1);
            using LinearGradientBrush brush = new LinearGradientBrush(area, top, bottom, LinearGradientMode.Vertical);
            g.FillRectangle(brush, area);
        }

        // Yıldızlar: hava karardıkça belirir, her biri kendi ritminde parlar
        private void DrawStars(Graphics g)
        {
            float visibility = Math.Clamp((1 - Brightness) * 2 - 0.6f, 0f, 1f);
            if (visibility <= 0)
                return;

            for (int i = 0; i < stars.Count; i++)
            {
                float twinkle = 0.6f + 0.4f * MathF.Sin(tick * 0.08f + starPhases[i]);
                int alpha = (int)(255 * visibility * twinkle);
                float size = (i % 3 == 0) ? 3 : 2;   // Bazı yıldızlar biraz daha büyük

                using SolidBrush starBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 230));
                g.FillEllipse(starBrush, stars[i].X, stars[i].Y, size, size);
            }
        }

        // Güneş ve ay aynı yay üzerinde, birbirinin tam karşısında döner
        private void DrawSunAndMoon(Graphics g)
        {
            float angle = cycle * MathF.PI * 2;
            DrawSun(g, OrbitPosition(angle));
            DrawMoon(g, OrbitPosition(angle + MathF.PI));   // 180° karşısında
        }

        // Açıya göre yay üzerindeki nokta
        private PointF OrbitPosition(float angle)
        {
            float x = width / 2f + MathF.Sin(angle) * width * 0.42f;
            float y = horizonY - MathF.Cos(angle) * horizonY * 0.75f;
            return new PointF(x, y);
        }

        private void DrawSun(Graphics g, PointF p)
        {
            if (p.Y > horizonY + 40)
                return;   // Ufkun altındaysa çizme

            using SolidBrush glow = new SolidBrush(Color.FromArgb(60, 255, 230, 120));
            using SolidBrush core = new SolidBrush(Color.FromArgb(255, 220, 80));

            g.FillEllipse(glow, p.X - 40, p.Y - 40, 80, 80);   // Işık halesi
            g.FillEllipse(core, p.X - 22, p.Y - 22, 44, 44);   // Güneş
        }

        private void DrawMoon(Graphics g, PointF p)
        {
            if (p.Y > horizonY + 40)
                return;

            using SolidBrush glow = new SolidBrush(Color.FromArgb(40, 255, 255, 230));
            using SolidBrush moon = new SolidBrush(Color.FromArgb(240, 240, 220));
            using SolidBrush crater = new SolidBrush(Color.FromArgb(215, 215, 195));

            g.FillEllipse(glow, p.X - 30, p.Y - 30, 60, 60);   // Hafif hale
            g.FillEllipse(moon, p.X - 18, p.Y - 18, 36, 36);   // Ay
            g.FillEllipse(crater, p.X - 8, p.Y - 9, 8, 8);     // Kraterler
            g.FillEllipse(crater, p.X + 3, p.Y + 2, 6, 6);
            g.FillEllipse(crater, p.X - 4, p.Y + 6, 4, 4);
        }

        // İki renk arasında karışım: t = 0 → a rengi, t = 1 → b rengi
        private static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
    }
}