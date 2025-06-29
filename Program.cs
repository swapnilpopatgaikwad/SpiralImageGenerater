using SkiaSharp;

namespace SpiralImageGenerater
{
    public enum GradientType
    {
        Linear,
        Radial,
        Conic,
        Diamond,
        Angular,
        Mesh,
        Bilinear,
        Sweep,
        Spiral,
        Noise
    }

    public static class ColorPalettes
    {
        public static SKColor[][] Palettes = new SKColor[][]
 {
    // 🌈 Vibrant Neon
    new[] { SKColor.Parse("#12c2e9"), SKColor.Parse("#c471ed"), SKColor.Parse("#f64f59") },
    new[] { SKColor.Parse("#f7971e"), SKColor.Parse("#ffd200"), SKColor.Parse("#ff416c") },

    // 🌌 Cyberpunk / Futuristic
    new[] { SKColor.Parse("#0f0c29"), SKColor.Parse("#302b63"), SKColor.Parse("#24243e") },
    new[] { SKColor.Parse("#833ab4"), SKColor.Parse("#fd1d1d"), SKColor.Parse("#fcb045") },

    // 🌅 Sunset Glow
    new[] { SKColor.Parse("#ee9ca7"), SKColor.Parse("#ffdde1"), SKColor.Parse("#ff6a00") },
    new[] { SKColor.Parse("#fcb045"), SKColor.Parse("#fd1d1d"), SKColor.Parse("#833ab4") },

    // 🌿 Nature + Aqua
    new[] { SKColor.Parse("#11998e"), SKColor.Parse("#38ef7d"), SKColor.Parse("#00c9ff") },
    new[] { SKColor.Parse("#43cea2"), SKColor.Parse("#185a9d"), SKColor.Parse("#2af598") },

    // 🎨 Pastel Blend
    new[] { SKColor.Parse("#fbc2eb"), SKColor.Parse("#a6c1ee"), SKColor.Parse("#d4fc79") },
    new[] { SKColor.Parse("#fddb92"), SKColor.Parse("#d1fdff"), SKColor.Parse("#fcb69f") },

    // 💎 Luxury Gold
    new[] { SKColor.Parse("#d4af37"), SKColor.Parse("#ffd700"), SKColor.Parse("#ffecb3") },
    new[] { SKColor.Parse("#3a1c71"), SKColor.Parse("#d76d77"), SKColor.Parse("#ffaf7b") },

    // ☁️ AI / Cloud Themes
    new[] { SKColor.Parse("#00d2ff"), SKColor.Parse("#3a7bd5"), SKColor.Parse("#00c6ff") },
    new[] { SKColor.Parse("#6a11cb"), SKColor.Parse("#2575fc"), SKColor.Parse("#00f2fe") },

    // 🌌 Deep Space + Purple
    new[] { SKColor.Parse("#000428"), SKColor.Parse("#004e92"), SKColor.Parse("#373B44") },
    new[] { SKColor.Parse("#41295a"), SKColor.Parse("#2F0743"), SKColor.Parse("#734b6d") },

    // 🔥 Tetradic Vibe
    new[] { SKColor.Parse("#ff6b6b"), SKColor.Parse("#ffe66d"), SKColor.Parse("#4ecdc4"), SKColor.Parse("#1a535c") },

    // 💗 Analogous Romance
    new[] { SKColor.Parse("#ff9a8b"), SKColor.Parse("#ff6b6b"), SKColor.Parse("#ff8e9d"), SKColor.Parse("#ff7eb3") }
 };


        public static SKColor[] GetRandomPaletteColors(Random rand)
        {
            return Palettes[rand.Next(Palettes.Length)];
        }

        public static SKColor[] GenerateRandomColors(Random rand, int count)
        {
            return Enumerable.Range(0, count)
                .Select(_ => new SKColor((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256)))
                .ToArray();
        }
    }

    public class GradientGenerator
    {
        public static void GenerateRandomGradientImages(
            int numberOfImages,
            int width = 1920,
            int height = 1080,
            GradientType gradientType = GradientType.Linear,
            bool useRandomColors = false,
            int? colorCount = null)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectPath = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName ?? currentDir;
            string outputDir = Path.Combine(projectPath, "GeneratedImages");
            Directory.CreateDirectory(outputDir);

            Random rand = new Random();
            var timer = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 1; i <= numberOfImages; i++)
            {
                SKColor[] colors = useRandomColors
                    ? ColorPalettes.GenerateRandomColors(rand, colorCount ?? rand.Next(2, 6))
                    : ColorPalettes.GetRandomPaletteColors(rand);

                using (SKSurface surface = SKSurface.Create(new SKImageInfo(width, height)))
                {
                    SKCanvas canvas = surface.Canvas;
                    canvas.Clear(SKColors.White);

                    switch (gradientType)
                    {
                        case GradientType.Spiral:
                            DrawSpiralGradient(canvas, width, height, colors);
                            break;
                    }

                    using (SKImage image = surface.Snapshot())
                    using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        string fileName = $"gradient_{gradientType}_{(useRandomColors ? "random" : "palette")}_{width}x{height}_{DateTime.Now:yyyyMMddHHmmss}.png";
                        string filePath = Path.Combine(outputDir, fileName);

                        using (FileStream stream = File.Create(filePath))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }
            }

            Console.WriteLine($"Generated {numberOfImages} {gradientType} images in {timer.Elapsed.TotalSeconds:F2}s");
        }

        private static void DrawSpiralGradient(SKCanvas canvas, int width, int height, SKColor[] colors)
        {
            var center = new SKPoint(width / 2f, height / 2f);
            float maxDist = Math.Min(width, height) * 0.5f;

            using (var bitmap = new SKBitmap(width, height))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float dx = x - center.X;
                        float dy = y - center.Y;
                        float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                        float angle = (float)Math.Atan2(dy, dx);
                        if (angle < 0) angle += 2 * (float)Math.PI;

                        float angleT = angle / (2 * (float)Math.PI); // [0, 1)
                        float distT = Math.Min(distance / maxDist, 1f);

                        // Pure modular blend: seamless loop from 0 to 1
                        float t = (angleT + distT) % 1f;

                        SKColor color = GetMultiColorGradient(colors, t);
                        bitmap.SetPixel(x, y, color);
                    }
                }

                using (var image = SKImage.FromBitmap(bitmap))
                {
                    canvas.DrawImage(image, 0, 0);
                }
            }
        }
        

        private static SKColor GetMultiColorGradient(SKColor[] colors, float t)
        {
            t = t % 1f;
            if (t < 0) t += 1f;

            if (colors.Length == 1) return colors[0];

            float segmentSize = 1f / colors.Length;
            int segmentIndex = (int)(t / segmentSize);
            int nextIndex = (segmentIndex + 1) % colors.Length;

            float localT = (t - segmentIndex * segmentSize) / segmentSize;
            return InterpolateColor(colors[segmentIndex], colors[nextIndex], localT);
        }

        private static SKColor InterpolateColor(SKColor c1, SKColor c2, float t)
        {
            return new SKColor(
                (byte)(c1.Red + (c2.Red - c1.Red) * t),
                (byte)(c1.Green + (c2.Green - c1.Green) * t),
                (byte)(c1.Blue + (c2.Blue - c1.Blue) * t)
            );
        }

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting image generation...");

            GradientGenerator.GenerateRandomGradientImages(
                    numberOfImages: 30,
                    width: 1200,
                    height: 800,
                    gradientType: GradientType.Spiral,
                    useRandomColors: false
                );

            Console.WriteLine("All images generated successfully!");
            Console.ReadLine();
        }
    }
}
