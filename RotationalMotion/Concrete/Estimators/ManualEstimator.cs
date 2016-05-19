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
	public class ManualEstimator : IRotationalMotionEstimator
	{
		private int dimention = 3;

		public AngularPositionModel Estimate(OpticalFlowModel opticalFlow)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<FlowModel> GetSpreadFlow(OpticalFlowModel opticalFlow)
		{
			List<FlowModel> spreadFlow = new List<FlowModel>();
			for (int i = 0; i < dimention; i++)
			{
				for (int j = 0; j < dimention; j++)
				{
					var minX = opticalFlow.Width/dimention*i;
					var maxX = opticalFlow.Width/dimention*(i + 1);
					var minY = opticalFlow.Height/dimention*j;
					var maxY = opticalFlow.Height/dimention*(j + 1);

					var flowPoints = opticalFlow.Flow.Where(f => f.Point.X < maxX && f.Point.X > minX && f.Point.Y < maxY && f.Point.Y > maxY);
					var superposition = GetSuperposition(flowPoints, new PointF(minX+maxX/2, minY+maxY/2));
					spreadFlow.Add(superposition);
				}
			}

			return spreadFlow;
		}

		protected FlowModel GetSuperposition(IEnumerable<FlowModel> flow, PointF point)
		{
			float xFLow, yFlow;

			if (flow.Count() != 0)
			{
				xFLow = flow.Sum(x => x.Flow.X) / flow.Count();
				yFlow = flow.Sum(y => y.Point.Y) / flow.Count();
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

		private double GetRoll(FlowModel[] flow)
		{
			var cells = dimention*dimention;
			var sum = cells - 1;

			for (int i = 0; i < cells/2; i++)
			{
				Math.Sign(i);
			}

			return 1;
		}
	}
}
