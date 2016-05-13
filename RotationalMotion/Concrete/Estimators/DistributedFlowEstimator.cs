using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotationalMotion.Abstract;
using RotationalMotion.Models;

namespace RotationalMotion.Concrete.Estimators
{
	public class DistributedFlowEstimator : SimpleEstimator, IRotationalMotionEstimator
	{
		public AngularPositionModel Estimate(OpticalFlowModel opticalFlow)
		{
			var distributedFlow = new OpticalFlowModel()
			{
				Width = opticalFlow.Width,
				Height = opticalFlow.Height,
				Flow = GetDistributedFlow(opticalFlow)
			};

			ResolvePartOfCoefficients(distributedFlow);

			var rotation = CalculateMatrix(distributedFlow);

			var result = new AngularPositionModel
			{
				Pitch = rotation[2, 0],
				Roll = rotation[0, 0],
				Yawing = rotation[1, 0]
			};

			return result;

		}

		protected IEnumerable<FlowModel> GetDistributedFlow(OpticalFlowModel opticalFlow)
		{
			var topFlow = opticalFlow.Flow.Where(v => v.Point.Y < opticalFlow.Height / 2);
			var bottomFlow = opticalFlow.Flow.Where(v => v.Point.Y > opticalFlow.Height / 2);
			var leftFlow = opticalFlow.Flow.Where(v => v.Point.X < opticalFlow.Width / 2);
			var rightFlow = opticalFlow.Flow.Where(v => v.Point.X > opticalFlow.Width / 2);

			var leftX = opticalFlow.Width/4;
			var midX = opticalFlow.Width/2;
			var rightX = opticalFlow.Width/4*3;
			var topY = (int) (opticalFlow.Width/4*3);
			var midY = (int) (opticalFlow.Width/2);
			var bottomY = (int) (opticalFlow.Width/4);

			var result = new List<FlowModel>
			{
				GetSuperposition(topFlow, new PointF(midX, topY)),
				GetSuperposition(bottomFlow, new PointF(midX, bottomY)),
				GetSuperposition(leftFlow, new PointF(leftX, midX)),
				GetSuperposition(rightFlow, new PointF(rightX, midY))
			};


			return result;
		}

		protected FlowModel GetSuperposition(IEnumerable<FlowModel> flow, PointF point)
		{
			float xFLow, yFlow;

			if (flow.Count() != 0)
			{
				xFLow = flow.Sum(x => x.Flow.X)/flow.Count();
				yFlow = flow.Sum(y => y.Point.Y)/flow.Count();
			}
			else
			{
				xFLow = yFlow = 0;
			}

			var result = new FlowModel()
			{
				Point = point,
				Flow = new PointF(xFLow, yFlow)
			};

			return result;
		}

		protected override void ResolvePartOfCoefficients(OpticalFlowModel opticalFlow)
		{
			a = b = c = d = e = f = 0;

			foreach (var flow in opticalFlow.Flow)
			{
				var x = flow.Point.X;
				var y = flow.Point.Y;

				a += (x * x * y * y + (y * y + 1) * (y * y + 1));
				b += ((x * x + 1) * (x * x + 1) + x * x * y * y);
				c += (x * x + y * y)* (x * x + y * y);
				d += (x * y * (x * x + y * y + 2));
				e += y;
				f += x;
			}


		}
	}
}
