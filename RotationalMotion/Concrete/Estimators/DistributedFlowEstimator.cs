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

			var leftX = (int) (opticalFlow.Width/4);
			var midX = (int) (opticalFlow.Width/2);
			var rightX = (int) (opticalFlow.Width/4*3);
			var topY = (int) (opticalFlow.Height/4*3);
			var midY = (int) (opticalFlow.Height/2);
			var bottomY = (int) (opticalFlow.Height/4);

			var result = new List<FlowModel>();

			result.Add(GetSuperposition(topFlow, new PointF(midX, topY)));
			result.Add(GetSuperposition(bottomFlow, new PointF(midX, bottomY)));
			result.Add(GetSuperposition(leftFlow, new PointF(leftX, midX)));
			result.Add(GetSuperposition(rightFlow, new PointF(rightX, midY)));

			return result;
		}

		protected FlowModel GetSuperposition(IEnumerable<FlowModel> flow, PointF point)
		{
			var xFLow = flow.Sum(x => x.Flow.X)/flow.Count();
			var yFlow = flow.Sum(y => y.Point.Y)/flow.Count();

			var result = new FlowModel()
			{
				Point = point,
				Flow = new PointF(xFLow, yFlow)
			};

			return result;
		} 
	}
}
