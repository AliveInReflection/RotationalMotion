using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

			var rotation = CalculateMatrix(opticalFlow);

			var flowArray = distributedFlow.Flow.ToArray();

			var pitch = Math.Abs(rotation[2, 0]);
			var roll = Math.Abs(rotation[0, 0]) * 1100;
			var yawing = -Math.Abs(rotation[1, 0]) * 1000;

			var result = new AngularPositionModel
			{
				Roll = HasPositiveRoll(flowArray) ? roll : HasNegativeRoll(flowArray) ? -roll : 0,
				Yawing = HasPositiveYawing(flowArray) ? yawing : HasNegativeYawing(flowArray) ? -yawing : 0,
				Pitch = HasPositivePitch(flowArray) ? pitch : HasNegativePitch(flowArray) ? -pitch : 0
			};

			return result;

		}

		protected IEnumerable<FlowModel> GetDistributedFlow(OpticalFlowModel opticalFlow)
		{
			var topFlow = opticalFlow.Flow.Where(v => v.Point.Y < opticalFlow.Height / 2);
			var bottomFlow = opticalFlow.Flow.Where(v => v.Point.Y > opticalFlow.Height / 2);
			var leftFlow = opticalFlow.Flow.Where(v => v.Point.X < opticalFlow.Width / 2);
			var rightFlow = opticalFlow.Flow.Where(v => v.Point.X > opticalFlow.Width / 2);

			var leftTopFlow = opticalFlow.Flow.Where(v => v.Point.Y < opticalFlow.Height/2 && v.Point.X < opticalFlow.Width/2);
			var rightTopFlow = opticalFlow.Flow.Where(v => v.Point.Y < opticalFlow.Height/2 && v.Point.X > opticalFlow.Width/2);
			var leftBottomFlow = opticalFlow.Flow.Where(v => v.Point.Y > opticalFlow.Height/2 && v.Point.X < opticalFlow.Width/2);
			var rightBottompFlow = opticalFlow.Flow.Where(v => v.Point.Y > opticalFlow.Height/2 && v.Point.X > opticalFlow.Width/2);

			var low = opticalFlow.Height/4;
			var mid = opticalFlow.Height / 2;
			var high = opticalFlow.Height / 4*3;

			var result = new List<FlowModel>
			{
				GetSuperposition(topFlow, new PointF(mid, low)),
				GetSuperposition(bottomFlow, new PointF(mid, high)),
				GetSuperposition(leftFlow, new PointF(low, mid)),
				GetSuperposition(rightFlow, new PointF(high, mid)),

				GetSuperposition(leftTopFlow, new PointF(low, low)),
				GetSuperposition(rightTopFlow, new PointF(high, low)),
				GetSuperposition(leftBottomFlow, new PointF(low, high)),
				GetSuperposition(rightBottompFlow, new PointF(high, high))
			};
			
			return result;
		}

		private bool HasPositiveRoll(FlowModel[] flow)
		{
			var hasRoll1 = (flow[4].Flow.X > 0 && flow[4].Flow.Y < 0 && flow[7].Flow.X < 0 && flow[7].Flow.Y > 0);
			var hasRoll2 = (flow[5].Flow.X > 0 && flow[5].Flow.Y > 0 && flow[6].Flow.X < 0 && flow[6].Flow.Y < 0);

			var hasRoll3 = (flow[4].Flow.X > 0 && flow[4].Flow.Y < 0 && flow[5].Flow.X > 0 && flow[5].Flow.Y > 0);
			var hasRoll4 = (flow[5].Flow.X > 0 && flow[5].Flow.Y > 0 && flow[7].Flow.X < 0 && flow[7].Flow.Y > 0);
			var hasRoll5 = (flow[7].Flow.X < 0 && flow[7].Flow.Y > 0 && flow[6].Flow.X < 0 && flow[6].Flow.Y < 0);
			var hasRoll6 = (flow[6].Flow.X < 0 && flow[6].Flow.Y < 0 && flow[4].Flow.X > 0 && flow[4].Flow.Y < 0);

			var hasRoll7 = (flow[0].Flow.X > 0 && flow[1].Flow.X < 0);
			var hasRoll8 = (flow[2].Flow.Y < 0 && flow[3].Flow.Y > 0);

			return hasRoll1 || hasRoll2 || hasRoll3 || hasRoll4 || hasRoll5 || hasRoll6 || hasRoll7 || hasRoll8;
		}

		private bool HasNegativeRoll(FlowModel[] flow)
		{
			var hasRoll1 = (flow[4].Flow.X < 0 && flow[4].Flow.Y > 0 && flow[7].Flow.X > 0 && flow[7].Flow.Y < 0);
			var hasRoll2 = (flow[5].Flow.X < 0 && flow[5].Flow.Y < 0 && flow[6].Flow.X > 0 && flow[6].Flow.Y > 0);

			var hasRoll3 = (flow[4].Flow.X < 0 && flow[4].Flow.Y > 0 && flow[5].Flow.X < 0 && flow[5].Flow.Y < 0);
			var hasRoll4 = (flow[5].Flow.X < 0 && flow[5].Flow.Y < 0 && flow[7].Flow.X > 0 && flow[7].Flow.Y < 0);
			var hasRoll5 = (flow[7].Flow.X > 0 && flow[7].Flow.Y < 0 && flow[6].Flow.X > 0 && flow[6].Flow.Y > 0);
			var hasRoll6 = (flow[6].Flow.X > 0 && flow[6].Flow.Y > 0 && flow[4].Flow.X < 0 && flow[4].Flow.Y > 0);

			var hasRoll7 = (flow[0].Flow.X < 0 && flow[1].Flow.X > 0);
			var hasRoll8 = (flow[2].Flow.Y > 0 && flow[3].Flow.Y < 0);

			return hasRoll1 || hasRoll2 || hasRoll3 || hasRoll4 || hasRoll5 || hasRoll6 || hasRoll7 || hasRoll8;
		}

		private bool HasPositivePitch(FlowModel[] flow)
		{
			var hasPitch1 = (flow[4].Flow.Y > 0 && flow[6].Flow.Y > 0);
			var hasPitch2 = (flow[5].Flow.Y > 0 && flow[7].Flow.Y > 0);
			var hasPitch3 = (flow[0].Flow.Y > 0 && flow[1].Flow.Y > 0);

			var hasRoll = HasPositiveRoll(flow) || HasNegativeRoll(flow);

			return (hasPitch1 || hasPitch2 || hasPitch3) && !hasRoll;
		}

		private bool HasNegativePitch(FlowModel[] flow)
		{
			var hasPitch1 = (flow[4].Flow.Y < 0 && flow[6].Flow.Y < 0);
			var hasPitch2 = (flow[5].Flow.Y < 0 && flow[7].Flow.Y < 0);
			var hasPitch3 = (flow[0].Flow.Y < 0 && flow[1].Flow.Y < 0);

			var hasRoll = HasPositiveRoll(flow) || HasNegativeRoll(flow);

			return (hasPitch1 || hasPitch2 || hasPitch3) && !hasRoll;
		}

		private bool HasPositiveYawing(FlowModel[] flow)
		{
			var hasYawing1 = (flow[4].Flow.X > 0 && flow[6].Flow.X > 0);
			var hasYawing2 = (flow[5].Flow.X > 0 && flow[7].Flow.X > 0);
			var hasYawing3 = (flow[0].Flow.X > 0 && flow[1].Flow.X > 0);

			var hasRoll = HasPositiveRoll(flow) || HasNegativeRoll(flow);

			return (hasYawing1 || hasYawing2 || hasYawing3) && !hasRoll;
		}

		private bool HasNegativeYawing(FlowModel[] flow)
		{
			var hasYawing1 = (flow[4].Flow.X < 0 && flow[6].Flow.X < 0);
			var hasYawing2 = (flow[5].Flow.X < 0 && flow[7].Flow.X < 0);
			var hasYawing3 = (flow[0].Flow.X < 0 && flow[1].Flow.X < 0);

			var hasRoll = HasPositiveRoll(flow) || HasNegativeRoll(flow);

			return (hasYawing1 || hasYawing2 || hasYawing3) && !hasRoll;
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
	}
}
