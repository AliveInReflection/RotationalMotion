using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using RotationalMotion.Abstract;
using RotationalMotion.Models;
using RotationalMotion.Utils;

namespace RotationalMotion.Concrete
{
    public class PyrLkOpticalFlowCalculator : IOpticalFlowCalculator
    {

        public IEnumerable<FlowModel> CalculateFlow(Image<Gray, byte> prev, Image<Gray, byte> cur)
        {
            var prevFeatures = prev.GoodFeaturesToTrack(100, 0.001d, 0.001d, 10);
            PointF[] currFeatures;
            var criteria = new MCvTermCriteria();
            byte[] status;
            float[] error;

            OpticalFlow.PyrLK(prev, cur, prevFeatures[0], new Size(10, 10), 1, criteria, out currFeatures, out status, out error);

            var result = new List<FlowModel>();

            for (int i = 0; i < prevFeatures[0].Length; i++)
            {
                if (status[i] == 1)
                {
                    result.Add(new FlowModel()
                    {
                        Point = prevFeatures[0][i],
                        Flow = currFeatures[i].Sub(prevFeatures[0][i])
                    });
                }
            }

            return result;
        }


    }
}
