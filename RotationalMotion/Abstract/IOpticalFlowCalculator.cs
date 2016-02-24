using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using RotationalMotion.Models;

namespace RotationalMotion.Abstract
{
    public interface IOpticalFlowCalculator
    {
        IEnumerable<FlowModel> CalculateFlow(Image<Gray, byte> prev, Image<Gray, byte> cur);
    }
}
