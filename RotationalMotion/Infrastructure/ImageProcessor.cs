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
using RotationalMotion.Concrete.Estimators;
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

        private IRotationalMotionEstimator _estimator;

        #region properties

        public double Roll { get; private set; }
        public double Pitch { get; private set; }
        public double Yawing { get; private set; }

        public int Width => _capture.Width;

        public int Height => _capture.Height;

        #endregion


        public ImageProcessor()
        {
            _capture = new Capture();
            _rotation = DenseMatrix.OfArray(new double[,] { { 0 }, { 0 }, { 0 } });
            _estimator = new SimpleEstimator();


            Roll = 0;
            Pitch = 0;
            Yawing = 0;
        }

        public ProcessingResult NextFrame(IOpticalFlowAlgorithm optFlowCalculator)
        {
            ProcessingResult result = null;

            try
            {
                _curFrame = _capture.QueryGrayFrame();
            }
            catch (AccessViolationException e)
            {

            }

            if (_prevFrame != null && _prevFrame.Data != null && _curFrame != null && _curFrame.Data != null)
            {
                try
                {
                    var optFlow = optFlowCalculator.CalculateFlow(_prevFrame, _curFrame).ToList();
                    CalculateRotation(optFlow);

                    var curFrame = _curFrame.Clone();
                    curFrame.DrawFlowVectors(optFlow);

                    result = new ProcessingResult()
                    {
                        Frame = curFrame.ToBitmap(),
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
                Frame = _curFrame.ToBitmap()
            });
        }

        #region change
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

        public void Reset()
        {
            Pitch = 0;
            Roll = 0;
            Yawing = 0;
        }
        #endregion

        #region privates

        private void CalculateRotation(IEnumerable<FlowModel> flow)
        {
            var motion = _estimator.Estimate(new OpticalFlowModel()
            {
                Flow = flow,
                Width = _capture.Width,
                Height = _capture.Height
            });

            Pitch += motion.Pitch;
            Roll += motion.Roll;
            Yawing += Yawing;
        }

        #endregion
    }
}
