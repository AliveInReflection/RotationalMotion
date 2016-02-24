using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using RotationalMotion.Models;

namespace RotationalMotion.Utils
{
    public static class Extensions
    {
        public static PointF Sub(this PointF left, PointF right)
        {
            return new PointF(left.X - right.X, left.Y - right.Y);
        }

        public static IEnumerable<FlowModel> Combine(this Image<Gray, float> flowX, Image<Gray, float> flowY, int step)
        {
            var result = new List<FlowModel>();

            for (int y = 0; y < flowX.Height; y += step)
            {
                for (int x = 0; x < flowX.Width; x += step)
                {
                    result.Add(new FlowModel()
                    {
                        Point = new PointF(x, y),
                        Flow = new PointF(flowX.Data[y,x,0], flowY.Data[y,x,0])
                    });
                }
            }

            return result;
        }

        public static double Abs(this double val)
        {
            return Math.Abs(val);
        }

        public static void Abs(this PointF point)
        {
            point.X = Math.Abs(point.X);
            point.Y = Math.Abs(point.Y);
        }

        public static double ToDegrees(this double val)
        {
            return Math.Round(val * 180 / Math.PI, 5);
        }

        public static BitmapImage ToImageSource(this Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static void DrawFlowVectors(this Image<Gray, byte> image, IEnumerable<FlowModel> flow)
        {
            foreach (var vector in flow)
            {
                var from = vector.Point;
                var to = new PointF(vector.Point.X + vector.Flow.X, vector.Point.Y + vector.Flow.Y);
                image.Draw(new LineSegment2DF(from,to), new Gray(1), 1);
            }
        }

        public static void DrawFeatures(this Image<Gray, byte> image, IEnumerable<FlowModel> flow)
        {
            foreach (var vector in flow)
            {
                image.Draw(new CircleF(vector.Point, 3), new Gray(1), 1);
            }
        }
    }
}
