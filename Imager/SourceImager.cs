using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Imager
{
    public class SourceImager
    {
        static Bitmap SAMPLE_BIT_MAP = new Bitmap(1, 1);
        static Graphics SAMPLE_GRAPHICS = Graphics.FromImage(SAMPLE_BIT_MAP);
        static StringFormat STRING_FORMAT = StringFormat.GenericDefault;
        static Color FOREGROUND = Color.FromArgb(211, 215, 207);
        static Color BACKGROUND = Color.FromArgb(46, 52, 54);
        static FontFamily[] FONT_FAMILIES = new[] { new FontFamily("Calibri"), new FontFamily("Cambria"), new FontFamily("Candara"), new FontFamily("Consolas"), new FontFamily("Constantia"), new FontFamily("Corbel"), new FontFamily("微软雅黑") };
        static Random rnd = new Random();
        static SourceImager()
        {
        }

        PointF RandomPoint(float width, float height)
        {
            return new PointF(width * (float)rnd.NextDouble(), height * (float)rnd.NextDouble());
        }

        public Image Generate(string code, int width, int maxHeight)
        {
            code = code.Replace("\t", "    ");

            FontFamily fontFamily = FONT_FAMILIES[rnd.Next(FONT_FAMILIES.Length)];
            Font font = new Font(fontFamily, rnd.Next(14, 17));
            SizeF size = SAMPLE_GRAPHICS.MeasureString(code, font, width, STRING_FORMAT);
            size.Height = Math.Min(maxHeight, size.Height);
            Bitmap bitmap = new Bitmap(width, (int)size.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(BACKGROUND);
                g.SmoothingMode = SmoothingMode.HighQuality;
                /*
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                 */
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.DrawString(code, font, new SolidBrush(FOREGROUND), new RectangleF(0, 0, size.Width, size.Height), STRING_FORMAT);

                /*
                for (int i = 0; i < 5; i++)
                {
                    g.DrawBezier(new Pen(Brushes.Black, 2), RandomPoint(bitmap.Width, bitmap.Height), RandomPoint(bitmap.Width, bitmap.Height), RandomPoint(bitmap.Width, bitmap.Height), RandomPoint(bitmap.Width, bitmap.Height));
                }
                 * */
                int numOfLines = bitmap.Height / 50;
                for (int i = 0; i < numOfLines; i++)
                {
                    g.DrawLine(new Pen(BACKGROUND, 2), RandomPoint(size.Width, size.Height), RandomPoint(size.Width, size.Height));
                }
            }
            return bitmap;
        }

        Bitmap Inverse(Bitmap bitmap)
        {
            Point center = new Point(rnd.Next(bitmap.Width), rnd.Next(bitmap.Height));
            double r = rnd.NextDouble() * 100;
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                    if ((center.X - i) * (center.X - i) + (center.Y - j) * (center.Y - j) <= r * r)
                    {
                        Color clr = bitmap.GetPixel(i, j);
                        clr = Color.FromArgb(255 - clr.R, 255 - clr.G, 255 - clr.B);
                        bitmap.SetPixel(i, j, clr);
                    }
            return bitmap;
        }

        Bitmap Interference(Bitmap bitmap)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            PointF center = PointF.Subtract(RandomPoint(bitmap.Width - 50, bitmap.Height - 50), new Size(-50, -50));
            double radius = 50;
            double C = 1.3, A = (1 - C) / (radius * radius);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    double deltaX = i - center.X, deltaY = j - center.Y;
                    double d2 = deltaX * deltaX + deltaY * deltaY;
                    double d = Math.Sqrt(d2);
                    if (d > radius)
                    {
                        newBitmap.SetPixel(i, j, bitmap.GetPixel(i, j));
                    }
                    else
                    {
                        double ratio = A * d2 + C;
                        newBitmap.SetPixel((int)(center.X + deltaX * ratio), (int)(center.Y + deltaY * ratio), bitmap.GetPixel(i, j));
                    }
                }
            }
            return newBitmap;
        }

        static void Main()
        {
            string src = ""
                + "#include <iostream>\n"
                + "int main() {\n"
                + "\tint x,y;\n"
                + "\tstd::cin >> x >> y;\n"
                + "\tstd::cout << x+y << std::endl;\n"
                + "\treturn 0;\n"
                + "}";
            src = File.ReadAllText("D:\\C++\\KMP.cpp");
            new SourceImager().Generate(src,1000,50000).Save("D:\\image.png", ImageFormat.Png);
        }
    }
}
