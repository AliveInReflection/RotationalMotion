using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private double _sensitivity = 0.0001;


        public ImageProcessor()
        {
            //_capture = new Capture("IzhFly.mp4");
            _capture = new Capture();
            _frameSource = new CaptureFrameSource(_capture);
            _stabilizer = new OnePassStabilizer(_frameSource);

            _rotation = DenseMatrix.OfArray(new double[,] { { 0 }, { 0 }, { 0 } });
        }

        public ProcessingResult NextFrame(IOpticalFlowAlgorithm optFlowCalculator)
        {
            ProcessingResult result = null;

            using (_stabilizer = new OnePassStabilizer(_frameSource))
            {
                _curFrame = _stabilize ? _stabilizer.NextFrame().Convert<Gray, byte>() : _capture.QueryGrayFrame();

                if (_prevFrame != null)
                {
                    try
                    {
                        var optFlow = optFlowCalculator.CalculateFlow(_prevFrame, _curFrame).ToList();
                        CalculateAngles(optFlow);

                        var prevFrame = _prevFrame.Clone();
                        var curFrame = _curFrame.Clone();

                        prevFrame.DrawFeatures(optFlow);
                        curFrame.DrawFlowVectors(optFlow);

                        result = new ProcessingResult()
                        {
                            Previous = prevFrame.ToBitmap(),
                            Current = curFrame.ToBitmap(),
                            Rotation = _rotation
                        };

                        prevFrame.Dispose();
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
            }

            return result ?? (result = new ProcessingResult()
            {
                Previous = _prevFrame.ToBitmap(),
                Current = _curFrame.ToBitmap(),
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

        private void CalculateAngles(IEnumerable<FlowModel> flow)
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
                var u = vector.Flow.X;
                var v = vector.Flow.Y;

                var x = vector.Point.X;
                var y = vector.Point.Y;

                a += x * x * y * y + (y * y + 1);
                b += (x * x + 1) + x * x * y * y;
                c += x * x + y * y;
                d += x * y * (x * x + y * y + 2);
                e += y;
                f += x;
                k += u * x * y + v * (y * y + 1);
                l += u * (x * x + 1) + v * x * y;
                m += u * y - v * x;
            }
                

            Matrix matrix33 = DenseMatrix.OfArray(new double[,] { { a, d, f }, { d, b, e }, { f, e, c } });
            Matrix matrix31 = DenseMatrix.OfArray(new double[,] { { k }, { l }, { m } });

            var result = matrix33.Inverse() * matrix31;

            //_rotation[0, 0] = result[0, 0];
            //_rotation[1, 0] = result[1, 0];
            //_rotation[2, 0] = result[2, 0];

            //_rotation[0, 0] += result[0, 0].Abs() > _sensitivity ? result[0, 0] : 0;
            //_rotation[1, 0] += result[1, 0].Abs() > _sensitivity ? result[1, 0] : 0;
            //_rotation[2, 0] += result[2, 0].Abs() > _sensitivity ? result[2, 0] : 0;

            _rotation[0, 0] += result[0, 0];
            _rotation[1, 0] += result[1, 0];
            _rotation[2, 0] += result[2, 0];
        }
    }
}
