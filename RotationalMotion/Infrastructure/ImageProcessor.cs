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
		private IRotationalMotionEstimator _estimator;
		private Capture _capture;

		private Image<Gray, byte> _curFrame;
		private Image<Gray, byte> _prevFrame;

		public event EventHandler FileEndRiched;


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
			_estimator = new IntagrationEstimator();
		}

		public ProcessingResult NextFrame(IOpticalFlowAlgorithm optFlowCalculator)
		{
			ProcessingResult result = null;
			try
			{
				if (_capture.Grab())
				{
					_curFrame = _capture.RetrieveGrayFrame();
				}

				if (_prevFrame?.Data != null && _curFrame?.Data != null)
				{
					var optFlow = optFlowCalculator.CalculateFlow(_prevFrame, _curFrame).ToList();


					var curFrame = _curFrame.Clone();
					curFrame.DrawFlowVectors(optFlow);

					var flowModel = new OpticalFlowModel(optFlow, Width, Height);

					var angularPosition = _estimator.Estimate(flowModel);
					result = new ProcessingResult(curFrame.ToBitmap(), angularPosition);
					curFrame.Dispose();
				}
			}
			catch (AggregateException e)
			{
				
			}
			catch (AccessViolationException e)
			{
				RiseFileEndRiched();
			}
			catch (Exception e)
			{

			}
			catch
			{
				
			}

			_prevFrame?.Dispose();
			if (_curFrame != null)
			{
				_prevFrame = _curFrame.Clone();
			}

			if (result == null)
			{
				result = new ProcessingResult(_curFrame.ToBitmap(), null);
			}

			return result;
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

		public void RiseFileEndRiched()
		{
			if (FileEndRiched != null)
			{
				RiseFileEndRiched();
			}
		}

		#endregion
	}
}
