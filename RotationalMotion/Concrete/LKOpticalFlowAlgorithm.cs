using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using RotationalMotion.Abstract;
using RotationalMotion.Models;
using RotationalMotion.Utils;

namespace RotationalMotion.Concrete
{
    public class LKOpticalFlowAlgorithm : IOpticalFlowAlgorithm
    {
        private int _step;
        private int _width;
        private int _height;

        public LKOpticalFlowAlgorithm(int width, int height, int step)
        {
            _height = height;
            _width = width;
            _step = step;
        }
        public IEnumerable<FlowModel> CalculateFlow(Image<Gray, byte> prev, Image<Gray, byte> cur)
        {
            Image<Gray, float> flowX = new Image<Gray, float>(_width, _height);
            Image<Gray, float> flowY = new Image<Gray, float>(_width, _height);

            OpticalFlow.LK(prev, cur, new Size(15, 15), flowX, flowY);

            return flowX.Combine(flowY, _step);
        }
    }
}
