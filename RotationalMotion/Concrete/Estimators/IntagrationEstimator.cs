using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotationalMotion.Models;

namespace RotationalMotion.Concrete.Estimators
{
	public class IntagrationEstimator : SimpleEstimator
	{
		private double prevPitch = 0;
		private double prevRoll = 0;
		private double prevYawing = 0;

		private double prevPitchVelocity = 0;
		private double prevRollVelocity = 0;
		private double prevYawingVelocity = 0;

		public IntagrationEstimator()
		{


		}

		public override AngularPositionModel Estimate(OpticalFlowModel opticalFlow)
		{
			ResolvePartOfCoefficients(opticalFlow);
			var velocities = CalculateMatrix(opticalFlow);

			var pitch = prevPitch - (prevPitchVelocity + 1)/1;
			var roll = prevRoll - (prevRollVelocity + 1)/1;
			var yawing = prevYawing - (prevYawingVelocity + 1)/1;

			var result = new AngularPositionModel(pitch, roll, yawing);

			prevRollVelocity = velocities[0, 0];
			prevYawingVelocity = velocities[1, 0];
			prevPitchVelocity = velocities[2, 0];

			return result;
		}
	}
}
