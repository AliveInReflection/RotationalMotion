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
    public class FarnebackOpticalFlowAlgorithm : IOpticalFlowAlgorithm
    {
        private int _step;
        private int _width;
        private int _height;

        public FarnebackOpticalFlowAlgorithm(int width, int height, int step)
        {
            _height = height;
            _width = width;
            _step = step;
        }

        public IEnumerable<FlowModel> CalculateFlow(Image<Gray, byte> prev, Image<Gray, byte> cur)
        {
            Image<Gray, float> flowX = new Image<Gray, float>(_width, _height);
            Image<Gray, float> flowY = new Image<Gray, float>(_width, _height);


            //prev – first 8-bit single-channel input image.
            //next – second input image of the same size and the same type as prev.
            //flow – computed flow image that has the same size as prev and type CV_32FC2.
            //pyr_scale – parameter, specifying the image scale (<1) to build pyramids for each image; pyr_scale=0.5 means a classical pyramid, where each next layer is twice smaller than the previous one.
            //levels – number of pyramid layers including the initial image; levels=1 means that no extra layers are created and only the original images are used.
            //winsize – averaging window size; larger values increase the algorithm robustness to image noise and give more chances for fast motion detection, but yield more blurred motion field.
            //iterations – number of iterations the algorithm does at each pyramid level.
            //poly_n – size of the pixel neighborhood used to find polynomial expansion in each pixel; larger values mean that the image will be approximated with smoother surfaces, yielding more robust algorithm and more blurred motion field, typically poly_n =5 or 7.
            //poly_sigma – standard deviation of the Gaussian that is used to smooth derivatives used as a basis for the polynomial expansion; for poly_n=5, you can set poly_sigma=1.1, for poly_n=7, a good value would be poly_sigma=1.5.
            //flags –
            OpticalFlow.Farneback(prev, cur, flowX, flowY, 0.5, 10, 20, 1, 7, 1.5, OPTICALFLOW_FARNEBACK_FLAG.FARNEBACK_GAUSSIAN);

            return flowX.Combine(flowY, _step);
        }
    }
}
