using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.VideoStab;
using MathNet.Numerics.LinearAlgebra.Double;
using RotationalMotion.Abstract;
using RotationalMotion.Models;
using RotationalMotion.Utils;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;


namespace RotationalMotion.Infrastructure
{
    public class ImageProcessor
    {
        private Capture _capture;
        private FrameSource _frameSource;
        private OnePassStabilizer _stabilizer;

        private Image<Gray, byte> _curFrame;
        private Image<Gray, byte> _prevFrame;

        private Matrix _rotation;

        private bool _stabilize = false;
        private double _sensitivity = 0.00001;

        public double Roll { get; private set; }
        public double Pitch { get; private set; }
        public double Yawing { get; private set; }

        public int Width => _capture.Width;

        public int Height => _capture.Height;


        public ImageProcessor()
        {
            _capture = new Capture("kiev.mp4");
            //_capture = new Capture();
            _frameSource = new CaptureFrameSource(_capture);
            _stabilizer = new OnePassStabilizer(_frameSource);

            _rotation = DenseMatrix.OfArray(new double[,] { { 0 }, { 0 }, { 0 } });


            Roll = 0;
            Pitch = 0;
            Yawing = 0;
        }

        public ProcessingResult NextFrame(IOpticalFlowAlgorithm optFlowCalculator)
        {
            ProcessingResult result = null;

            try
            {
                _curFrame = _stabilize ? _stabilizer.NextFrame().Convert<Gray, byte>() : _capture.QueryGrayFrame();
            }
            catch (AccessViolationException e)
            {

            }

            if (_prevFrame != null && _prevFrame.Data != null)
            {
                try
                {
                    var optFlow = optFlowCalculator.CalculateFlow(_prevFrame, _curFrame).ToList();

                    var genearalFlow = CalculateGeneralFlow(optFlow);

                    CalculateAngles(optFlow);


                    var curFrame = _curFrame.Clone();
                    curFrame.DrawFlowVectors(optFlow);

                    result = new ProcessingResult()
                    {
                        Frame = curFrame.ToBitmap(),
                        Rotation = _rotation
                    };

                    curFrame.Dispose();
                }
                catch (Exception e)
                {

                }
            }

            if (_prevFrame != null)
            {
                _prevFrame.Dispose();
            }
            _prevFrame = _curFrame.Clone();


            return result ?? (result = new ProcessingResult()
            {
                Frame = _curFrame.ToBitmap(),
                Rotation = _rotation
            });
        }

        public void ChangeCapture(int device = 0)
        {
            _capture.Dispose();
            _capture = new Capture(device);
            _capture.Grab();
        }

        public void ChangeCapture(string filePath)
        {
            _capture.Dispose();
            _capture = new Capture(filePath);
            _capture.Grab();
        }

        private void CalculateAngles(List<FlowModel> flow)
        {
            var topFlow = flow.Where(m => m.Point.Y <= _capture.Height / 2).ToList();
            var bottomFlow = flow.Where(m => m.Point.Y > _capture.Height/2). ToList();
            var leftFlow = flow.Where(m => m.Point.X <= _capture.Width/2).ToList();
            var rightFlow = flow.Where(m => m.Point.X > _capture.Width/2).ToList();

            var topPitch = CalculateAngle(topFlow, Angles.Pitch);
            var bottomPitch = CalculateAngle(bottomFlow, Angles.Pitch);
            var leftPitch = CalculateAngle(leftFlow, Angles.Pitch);
            var rightPitch = CalculateAngle(rightFlow, Angles.Pitch);

            var topYawing = CalculateAngle(topFlow, Angles.Yawing);
            var bottomYawing = CalculateAngle(bottomFlow, Angles.Yawing);
            var leftYawing = CalculateAngle(leftFlow, Angles.Yawing);
            var rightYawing = CalculateAngle(rightFlow, Angles.Yawing);





            var pitch = (((topPitch + bottomPitch) / 2) + ((leftPitch + rightPitch)/2))/2;
            Pitch += pitch.Abs() >= _sensitivity ? pitch : 0;

            var yawing = (((topYawing + bottomYawing) / 2) + ((leftYawing + rightYawing)/2))/2;
            Yawing += yawing.Abs() >= _sensitivity ? yawing : 0;

        }

        public IEnumerable<FlowModel> CalculateGeneralFlow(List<FlowModel> flow)
        {
            var topFlow = flow.Where(m => m.Point.Y <= _capture.Height / 2).ToList();
            var bottomFlow = flow.Where(m => m.Point.Y > _capture.Height / 2).ToList();
            var leftFlow = flow.Where(m => m.Point.X <= _capture.Width / 2).ToList();
            var rightFlow = flow.Where(m => m.Point.X > _capture.Width / 2).ToList();

            var list = new List<FlowModel>();
            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 2, _capture.Height / 4),
                Flow = new PointF(topFlow.Sum(m => m.Flow.X) / topFlow.Count, 0)
            });
            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 2, _capture.Height / 4),
                Flow = new PointF(0, topFlow.Sum(m => m.Flow.Y) / topFlow.Count)
            });


            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 2, _capture.Height / 4 * 3),
                Flow = new PointF(bottomFlow.Sum(m => m.Flow.X) / bottomFlow.Count, 0)
            });
            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 2, _capture.Height / 4 * 3),
                Flow = new PointF(0, bottomFlow.Sum(m => m.Flow.Y) / bottomFlow.Count)
            });


            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 4, _capture.Height / 2),
                Flow = new PointF(leftFlow.Sum(m => m.Flow.X) / leftFlow.Count, 0)
            });
            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 4, _capture.Height / 2),
                Flow = new PointF(0, leftFlow.Sum(m => m.Flow.Y) / leftFlow.Count)
            });


            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 4 * 3, _capture.Height / 2),
                Flow = new PointF(rightFlow.Sum(m => m.Flow.X) / rightFlow.Count, 0)
            });
            list.Add(new FlowModel()
            {
                Point = new PointF(_capture.Width / 4 * 3, _capture.Height / 2),
                Flow = new PointF(0, rightFlow.Sum(m => m.Flow.Y) / rightFlow.Count)
            });

            return list;
        }

        private double CalculateAngle(List<FlowModel> flow, Angles angle)
        {
            double a = 0;
            double b = 0;
            double c = 0;
            double d = 0;
            double e = 0;
            double f = 0;
            double k = 0;
            double l = 0;
            double m = 0;

            foreach (var vector in flow)
            {
                var u = angle == Angles.Yawing ? vector.Flow.X : 0;
                var v = angle == Angles.Pitch ? vector.Flow.Y : 0;

                var x = vector.Point.X;
                var y = vector.Point.Y;

                a += (x * x * y * y + (y * y + 1));
                b += ((x * x + 1) + x * x * y * y);
                c += (x * x + y * y);
                d += (x * y * (x * x + y * y + 2));
                e += y;
                f += x;
                k += (u * x * y + v * (y * y + 1));
                l += (u * (x * x + 1) + v * x * y);
                m += (u * y - v * x);
            }


            Matrix matrix33 = DenseMatrix.OfArray(new double[,] { { a, d, f },
                                                                  { d, b, e },
                                                                  { f, e, c } });


            Matrix matrix31 = DenseMatrix.OfArray(new double[,] { { k }, { l }, { m } });

            var rotation = matrix33.Inverse() * matrix31;


            return rotation[2, 0];
        }

        public void Reset()
        {
            Pitch = 0;
            Roll = 0;
            Yawing = 0;
        }
    }
}
