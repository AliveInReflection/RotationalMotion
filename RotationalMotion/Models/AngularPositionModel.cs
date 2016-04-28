using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotationalMotion.Models
{
	public class AngularPositionModel
	{
		public AngularPositionModel()
		{
			
		}

		public AngularPositionModel(double pitch, double roll, double yawing)
		{
			Pitch = pitch;
			Roll = roll;
			Yawing = yawing;
		}

		public double Pitch { get; set; }

		public double Roll { get; set; }

		public double Yawing { get; set; }
	}
}
