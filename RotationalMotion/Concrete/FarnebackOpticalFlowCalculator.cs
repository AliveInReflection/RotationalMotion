using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RotationalMotion.Abstract;
using RotationalMotion.Models;
using RotationalMotion.Utils;

namespace RotationalMotion.Concrete
{
    public class FarnebackOpticalFlowCalculator : IOpticalFlowCalculator
    {
        private int _step;
        private int _width;
        private int _height;

        public FarnebackOpticalFlowCalculator(int width, int height, int step)
        {
            _height = height;
            _width = width;
            _step = step;
        }

        public IEnumerable<FlowModel> CalculateFlow(Image<Gray, byte> prev, Image<Gray, byte> cur)
        {
            Image<Gray, float> flowX = new Image<Gray, float>(_width, _height);
            Image<Gray, float> flowY = new Image<Gray, float>(_width, _height);

            OpticalFlow.Farneback(prev, cur, flowX, flowY, 0.1, 2, 4, 1, 2, 1.2, OPTICALFLOW_FARNEBACK_FLAG.FARNEBACK_GAUSSIAN);

            return flowX.Combine(flowY, _step);
        }
    }
}
