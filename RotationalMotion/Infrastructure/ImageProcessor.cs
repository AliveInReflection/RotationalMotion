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
            var pitch = CalculateAngle(flow, Angles.Pitch);
            Pitch += pitch.Abs() >= _sensitivity ? pitch : 0;

            var yawing = CalculateAngle(flow, Angles.Yawing);
            Yawing += yawing.Abs() >= _sensitivity ? yawing : 0;

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
