using SkiaSharp;
using SkiaSharp.HarfBuzz;

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
            new[] { SKColor.Parse("#12c2e9"), SKColor.Parse("#c471ed"), SKColor.Parse("#f64f59") },
            new[] { SKColor.Parse("#f7971e"), SKColor.Parse("#ffd200"), SKColor.Parse("#ff416c") },
            new[] { SKColor.Parse("#0f0c29"), SKColor.Parse("#302b63"), SKColor.Parse("#24243e") },
            new[] { SKColor.Parse("#833ab4"), SKColor.Parse("#fd1d1d"), SKColor.Parse("#fcb045") },
            new[] { SKColor.Parse("#ee9ca7"), SKColor.Parse("#ffdde1"), SKColor.Parse("#ff6a00") },
            new[] { SKColor.Parse("#fcb045"), SKColor.Parse("#fd1d1d"), SKColor.Parse("#833ab4") },
            new[] { SKColor.Parse("#11998e"), SKColor.Parse("#38ef7d"), SKColor.Parse("#00c9ff") },
            new[] { SKColor.Parse("#43cea2"), SKColor.Parse("#185a9d"), SKColor.Parse("#2af598") },

            new[] { SKColor.Parse("#fbc2eb"), SKColor.Parse("#a6c1ee"), SKColor.Parse("#d4fc79") },
            new[] { SKColor.Parse("#fddb92"), SKColor.Parse("#d1fdff"), SKColor.Parse("#fcb69f") },

            new[] { SKColor.Parse("#d4af37"), SKColor.Parse("#ffd700"), SKColor.Parse("#ffecb3") },
            new[] { SKColor.Parse("#3a1c71"), SKColor.Parse("#d76d77"), SKColor.Parse("#ffaf7b") },

            new[] { SKColor.Parse("#00d2ff"), SKColor.Parse("#3a7bd5"), SKColor.Parse("#00c6ff") },
            new[] { SKColor.Parse("#6a11cb"), SKColor.Parse("#2575fc"), SKColor.Parse("#00f2fe") },

            new[] { SKColor.Parse("#000428"), SKColor.Parse("#004e92"), SKColor.Parse("#373B44") },
            new[] { SKColor.Parse("#41295a"), SKColor.Parse("#2F0743"), SKColor.Parse("#734b6d") },

            new[] { SKColor.Parse("#ff6b6b"), SKColor.Parse("#ffe66d"), SKColor.Parse("#4ecdc4"), SKColor.Parse("#1a535c") },

            new[] { SKColor.Parse("#ff9a8b"), SKColor.Parse("#ff6b6b"), SKColor.Parse("#ff8e9d"), SKColor.Parse("#ff7eb3") },

            new[] { SKColor.Parse("#a1c4fd"), SKColor.Parse("#c2e9fb"), SKColor.Parse("#d4fc79") },
            new[] { SKColor.Parse("#fddb92"), SKColor.Parse("#d1fdff"), SKColor.Parse("#e0c3fc") },

            new[] { SKColor.Parse("#a8edea"), SKColor.Parse("#fed6e3"), SKColor.Parse("#dbe6e4") },
            new[] { SKColor.Parse("#e6e9f0"), SKColor.Parse("#eef1f5"), SKColor.Parse("#f8f9fa") },

            new[] { SKColor.Parse("#e0c3fc"), SKColor.Parse("#8ec5fc"), SKColor.Parse("#cfd9df") },
            new[] { SKColor.Parse("#f6e27a"), SKColor.Parse("#fbc687"), SKColor.Parse("#fbe3d0") },

            new[] { SKColor.Parse("#dee2e6"), SKColor.Parse("#ced4da"), SKColor.Parse("#adb5bd") },
            new[] { SKColor.Parse("#89f7fe"), SKColor.Parse("#66a6ff"), SKColor.Parse("#91eae4") },

            new[] { SKColor.Parse("#ffe1e8"), SKColor.Parse("#fad0c4"), SKColor.Parse("#ffd1ff") },
            new[] { SKColor.Parse("#fddde6"), SKColor.Parse("#fbc8d4"), SKColor.Parse("#f7e9e3") }
        };

        public static SKColor[] GetRandomPaletteColors(Random rand) => Palettes[rand.Next(Palettes.Length)];

        public static SKColor[] GenerateRandomColors(Random rand, int count) => Enumerable.Range(0, count).Select(_ => new SKColor((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256))).ToArray();

        public static SKColor HSLToColor(float h, float s, float l)
        {
            h = h % 360;
            float c = (1 - Math.Abs(2 * l - 1)) * s;
            float x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            float m = l - c / 2;

            float r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; }
            else if (h < 120) { r = x; g = c; }
            else if (h < 180) { g = c; b = x; }
            else if (h < 240) { g = x; b = c; }
            else if (h < 300) { r = x; b = c; }
            else { r = c; b = x; }

            return new SKColor(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }
        public static SKColor[] GenerateAnalogousPalette(Random rand, int count = 3)
        {
            float baseHue = (float)(rand.NextDouble() * 360);
            return Enumerable.Range(0, count).Select(i => HSLToColor((baseHue + i * 20f) % 360, 0.6f, 0.7f)).ToArray();
        }

        public static SKColor[] GenerateSoftPastelPalette(Random rand, int count = 3)
        {
            return Enumerable.Range(0, count).Select(_ =>
            {
                byte r = (byte)rand.Next(180, 256);
                byte g = (byte)rand.Next(180, 256);
                byte b = (byte)rand.Next(180, 256);
                return new SKColor(r, g, b);
            }).ToArray();
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
         int? colorCount = null,
         bool isSoft = true,
         string brandText = "",
         string thoughtText = "",
         bool isMarathi = true)

        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectPath = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName ?? currentDir;
            string outputDir = Path.Combine(projectPath, "GeneratedImages");
            Directory.CreateDirectory(outputDir);

            Random rand = new Random();
            var timer = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 1; i <= numberOfImages; i++)
            {
                SKColor[] colors = isSoft
                    ? (useRandomColors ? ColorPalettes.GenerateSoftPastelPalette(rand, colorCount ?? 3) : ColorPalettes.GenerateAnalogousPalette(rand, colorCount ?? 3))
                    : (useRandomColors ? ColorPalettes.GenerateRandomColors(rand, colorCount ?? 3) : ColorPalettes.GetRandomPaletteColors(rand));

                using var surface = SKSurface.Create(new SKImageInfo(width, height));
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                if (gradientType == GradientType.Spiral)
                    DrawSpiralGradient(canvas, width, height, colors, brandText, thoughtText, isMarathi);

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                string fileName = $"gradient_{gradientType}_{DateTime.Now:yyyyMMddHHmmss}.png";
                using var stream = File.Create(Path.Combine(outputDir, fileName));
                data.SaveTo(stream);
            }

            Console.WriteLine($"Generated {numberOfImages} {gradientType} image(s) in {timer.Elapsed.TotalSeconds:F2}s");
        }

        private static void DrawSpiralGradient(SKCanvas canvas, int width, int height, SKColor[] colors, string brandText, string thoughtText, bool isMarathi)
        {
            var center = new SKPoint(width / 2f, height / 2f);
            float maxDist = Math.Min(width, height) * 0.5f;
            using var bitmap = new SKBitmap(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float dx = x - center.X, dy = y - center.Y;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float angle = (float)Math.Atan2(dy, dx);
                    if (angle < 0) angle += 2 * MathF.PI;

                    float angleT = angle / (2 * MathF.PI);
                    float distT = Math.Min(distance / maxDist, 1f);
                    float t = (angleT + distT) % 1f;

                    SKColor color = GetMultiColorGradient(colors, t);
                    bitmap.SetPixel(x, y, color);
                }

            using var image = SKImage.FromBitmap(bitmap);
            canvas.DrawImage(image, 0, 0);
            DrawBrandText(canvas, width, height, brandText);
            DrawThoughtText(canvas, width, height, thoughtText, isMarathi);
        }

        private static SKColor GetMultiColorGradient(SKColor[] colors, float t)
        {
            t = t % 1f;
            if (t < 0) t += 1f;

            if (colors.Length == 1) return colors[0];

            float segmentSize = 1f / colors.Length;
            int index = (int)(t / segmentSize);
            int next = (index + 1) % colors.Length;
            float localT = (t - index * segmentSize) / segmentSize;

            return InterpolateColor(colors[index], colors[next], localT);
        }

        private static SKColor InterpolateColor(SKColor c1, SKColor c2, float t)
        {
            return new SKColor(
                (byte)(c1.Red + (c2.Red - c1.Red) * t),
                (byte)(c1.Green + (c2.Green - c1.Green) * t),
                (byte)(c1.Blue + (c2.Blue - c1.Blue) * t)
            );
        }

        private static void DrawBrandText(SKCanvas canvas, int width, int height, string brandText)
        {
            if (string.IsNullOrWhiteSpace(brandText)) return;
            float size = width * 0.02f;
            using var paint = new SKPaint { Color = SKColors.White.WithAlpha(230), TextSize = size, IsAntialias = true, TextAlign = SKTextAlign.Center, Typeface = GetStylishTypeface(), FakeBoldText = true };
            using var shadow = paint.Clone();
            shadow.Color = SKColors.Black.WithAlpha(100);
            float x = width / 2f, y = height - size * 1.2f;
            canvas.DrawText(brandText, x + 2, y + 2, shadow);
            canvas.DrawText(brandText, x, y, paint);
        }

        private static void DrawThoughtText(SKCanvas canvas, int width, int height, string thought, bool isMarathi)
        {
            if (isMarathi)
                DrawMarathiThought(canvas, width, height, thought);
            else
                DrawEnglishThought(canvas, width, height, thought);
        }

        private static void DrawEnglishThought(SKCanvas canvas, int width, int height, string thought)
        {
            if (string.IsNullOrWhiteSpace(thought)) return;

            float textSize = 44f;
            float quoteSize = textSize * 2.15f;
            int maxWidth = (int)(width * 0.9);
            var typeface = GetStylishTypeface();

            using var font = new SKFont(typeface, textSize);
            using var quoteFont = new SKFont(typeface, quoteSize);

            using var paint = new SKPaint { Color = SKColors.White.WithAlpha(240), IsAntialias = true };
            using var quotePaint = new SKPaint { Color = SKColors.White.WithAlpha(240), IsAntialias = true };
            using var shadow = new SKPaint { Color = SKColors.Black.WithAlpha(120), IsAntialias = true };
            using var quoteShadow = new SKPaint { Color = SKColors.Black.WithAlpha(120), IsAntialias = true };

            using var shaper = new SKShaper(typeface);
            var lines = WrapText(thought, font, shaper, maxWidth);

            float lineSpacing = textSize * 1.6f;
            float quoteLineHeight = quoteSize * 1.1f;
            float totalTextHeight = lines.Count * lineSpacing;

            float blockPadding = 20f;
            float blockWidth = maxWidth + blockPadding * 2;
            float blockHeight = totalTextHeight + quoteLineHeight + blockPadding * 2;

            float blockX = (width - blockWidth) / 2f;
            float blockY = (height - blockHeight) / 2f;

            var backgroundPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 100),
                IsAntialias = true
            };
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(blockX, blockY, blockX + blockWidth, blockY + blockHeight), 20f, 20f), backgroundPaint);

            // Opening Quote “ (top-left inside block)
            string openQuote = "“";
            var quoteOpenBlob = SKTextBlob.Create(openQuote, quoteFont);
            float qx1 = blockX + 10;
            float qy1 = blockY + quoteOpenBlob.Bounds.Height;
            canvas.DrawText(quoteOpenBlob, qx1 + 2, qy1 + 2, quoteShadow);
            canvas.DrawText(quoteOpenBlob, qx1, qy1, quotePaint);

            float startY = blockY + quoteLineHeight + blockPadding;
            SKRect lastLineBounds = SKRect.Empty;

            for (int i = 0; i < lines.Count; i++)
            {
                var shaped = shaper.Shape(lines[i], font);
                var builder = new SKTextBlobBuilder();
                var run = builder.AllocatePositionedRun(font, shaped.Codepoints.Length);
                for (int j = 0; j < shaped.Codepoints.Length; j++)
                {
                    run.Glyphs[j] = (ushort)shaped.Codepoints[j];
                    run.Positions[j] = shaped.Points[j];
                }
                var blob = builder.Build();
                float x = width / 2f - blob.Bounds.MidX;
                float y = startY + i * lineSpacing;

                canvas.DrawText(blob, x + 2, y + 2, shadow);
                canvas.DrawText(blob, x, y, paint);

                if (i == lines.Count - 1)
                    lastLineBounds = new SKRect(x, y - blob.Bounds.Height, x + blob.Bounds.Width, y);
            }

            // Closing Quote ” at the end of the last line
            string closeQuote = "”";
            var quoteCloseBlob = SKTextBlob.Create(closeQuote, quoteFont);
            float qx2 = lastLineBounds.Right + 10;
            float qy2 = lastLineBounds.Top + quoteCloseBlob.Bounds.Height * 0.9f;

            canvas.DrawText(quoteCloseBlob, qx2 + 2, qy2 + 2, quoteShadow);
            canvas.DrawText(quoteCloseBlob, qx2, qy2, quotePaint);
        }

        private static void DrawMarathiThought(SKCanvas canvas, int width, int height, string thought)
        {
            if (string.IsNullOrWhiteSpace(thought)) return;

            float size = 44f;
            float horizontalMargin = width * 0.08f;  // 8% left-right margin
            int maxWidth = (int)(width - (horizontalMargin * 2));
            var typeface = GetMarathiTypeface();

            thought = "\u201C\u00A0" + "  " + thought +"  "+ "\u00A0\u201D"; // “ thought ”

            using var font = new SKFont(typeface, size);
            using var paint = new SKPaint { Color = SKColors.White.WithAlpha(240), IsAntialias = true };
            using var shadow = new SKPaint { Color = SKColors.Black.WithAlpha(120), IsAntialias = true };
            using var shaper = new SKShaper(typeface);

            var lines = WrapText(thought, font, shaper, maxWidth);
            float spacing = size * 1.6f;
            float yStart = height / 2f - (lines.Count * spacing) / 2f + spacing / 2f;

            float maxLineWidth = 0;
            List<SKTextBlob> blobs = new();
            foreach (var line in lines)
            {
                var result = shaper.Shape(line, font);
                var builder = new SKTextBlobBuilder();
                var run = builder.AllocatePositionedRun(font, result.Codepoints.Length);
                for (int j = 0; j < result.Codepoints.Length; j++)
                {
                    run.Glyphs[j] = (ushort)result.Codepoints[j];
                    run.Positions[j] = result.Points[j];
                }
                var blob = builder.Build();
                blobs.Add(blob);
                maxLineWidth = Math.Max(maxLineWidth, blob.Bounds.Width);
            }

            float blockPadding = 20f;
            float blockWidth = maxLineWidth + blockPadding * 2;
            float blockHeight = lines.Count * spacing + blockPadding * 2;
            float blockX = (width - blockWidth) / 2f;
            float blockY = yStart - spacing / 2f - blockPadding;

            var backgroundPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 100),
                IsAntialias = true
            };
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(blockX, blockY, blockX + blockWidth, blockY + blockHeight), 20f, 20f), backgroundPaint);

            for (int i = 0; i < blobs.Count; i++)
            {
                var blob = blobs[i];
                float x = width / 2f - blob.Bounds.MidX;
                float y = yStart + i * spacing;
                canvas.DrawText(blob, x + 2, y + 2, shadow);
                canvas.DrawText(blob, x, y, paint);
            }
        }

        private static List<string> WrapText(string text, SKFont font, SKShaper shaper, int maxWidth)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            string current = "";

            foreach (var word in words)
            {
                string test = string.IsNullOrEmpty(current) ? word : current + " " + word;
                if (shaper.Shape(test, font).Width <= maxWidth)
                    current = test;
                else
                {
                    if (!string.IsNullOrEmpty(current)) lines.Add(current);
                    current = word;
                }
            }

            if (!string.IsNullOrEmpty(current)) lines.Add(current);
            return lines;
        }

        private static SKTypeface GetStylishTypeface()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectPath = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName ?? currentDir;
            string path = Path.Combine(projectPath, "Fonts", "LibreBaskerville-Italic.ttf");
            return File.Exists(path) ? SKTypeface.FromFile(path) : SKTypeface.Default;
        }

        private static SKTypeface GetMarathiTypeface()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string projectPath = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName ?? currentDir;
            string path = Path.Combine(projectPath, "Fonts", "Poppins-BoldItalic.ttf");
            return File.Exists(path) ? SKTypeface.FromFile(path) : SKTypeface.Default;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Generating gradient image with quote...");

            string brand = "@DevWithSwap";
            string thought = "प्रत्येक क्षणात आनंद शोधा आणि आयुष्य साजरे करा";
            //string thought = "Success is not final, failure is not fatal: It is the courage to continue that counts.";
            bool isMarathi = true;

            GradientGenerator.GenerateRandomGradientImages(
                numberOfImages: 1,
                width: 1200,
                height: 800,
                gradientType: GradientType.Spiral,
                useRandomColors: false,
                isSoft: true,
                brandText: brand,
                thoughtText: thought,
                isMarathi: isMarathi
            );

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}