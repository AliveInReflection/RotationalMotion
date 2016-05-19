using System.Collections.Generic;
using RotationalMotion.Abstract;
using RotationalMotion.Models;

using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace RotationalMotion.Concrete.Estimators
{
	public class SimpleEstimator : IRotationalMotionEstimator
	{
		protected double a, b, c, d, e, f, k, l, m;

		public virtual AngularPositionModel Estimate(OpticalFlowModel opticalFlow)
		{

			//ResolvePartOfCoefficients(opticalFlow);

			var rotation = CalculateMatrix(opticalFlow);

			var result = new AngularPositionModel
			{
				Pitch = rotation[2, 0],
				Roll = rotation[0, 0],
				Yawing = rotation[1, 0]
			};

			return result;
		}

		protected virtual Matrix CalculateMatrix(OpticalFlowModel opticalFlow)
		{
			k = l = m = a = b = c = d = e = f = 0;
			foreach (var vector in opticalFlow.Flow)
			{
				var u = vector.Flow.X;
				var v = - vector.Flow.Y;

				var x = vector.Point.X;
				var y = opticalFlow.Height - vector.Point.Y;

				a += (x * x * y * y + (y * y + 1) * (y * y + 1));
				b += ((x * x + 1) * (x * x + 1) + x * x * y * y);
				c += (x * x + y * y);
				d -= (x * y * (x * x + y * y + 2));
				e -= y;
				f -= x;


				k += (u * x * y + v * (y * y + 1));
				l -= (u * (x * x + 1) + v * x * y);
				m += (u * y - v * x);
			}

			//Matrix matrix33 = DenseMatrix.OfArray(new double[,] { { f, d, a },
			//													  { e, b, d },
			//													  { c, e, f } });
			Matrix matrix33 = DenseMatrix.OfArray(new double[,] { { a, d, f },
																  { d, b, e },
																  { f, e, c } });

			Matrix matrix31 = DenseMatrix.OfArray(new double[,] { { k }, { l }, { m } });

			var rotation = matrix33.Inverse() * matrix31;

			return rotation;
		}

		protected virtual void ResolvePartOfCoefficients(OpticalFlowModel opticalFlow)
		{

			if (opticalFlow.Width == 1280 && opticalFlow.Height == 720)
			{
				a = 42081547840512;
				b = 33176187759616;
				c = 66164766720000;
				d = -29762523293696;
				e = -331315200;
				f = -589363200;
			}
			else if (opticalFlow.Width == 800 && opticalFlow.Height == 600)
			{
				a = 92585546335538;
				b = 87337458639829;
				c = 36822347864300;
				d = -87278472889012;
				e = -366879328;
				f = -441940593;
			}
			else
			{
				for (int x = 0; x < opticalFlow.Width; x++)
				{
					for (int y = 0; y < opticalFlow.Height; y++)
					{
						a += (x * x * y * y + (y * y + 1) * (y * y + 1));
						b += ((x * x + 1) * (x * x + 1) + x * x * y * y);
						c += (x * x + y * y);
						d += (x * y * (x * x + y * y + 2));
						e += y;
						f += x;
					}
				}
			}

		}
	}
}
