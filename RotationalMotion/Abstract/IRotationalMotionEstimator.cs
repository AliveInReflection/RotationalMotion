using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotationalMotion.Models;

namespace RotationalMotion.Abstract
{
    public interface IRotationalMotionEstimator
    {
        AngularPositionModel Estimate(OpticalFlowModel opticalFlow);
    }
}
