using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RotationalMotion.Abstract;
using RotationalMotion.Models;
using RotationalMotion.Utils;

namespace RotationalMotion.Concrete
{
    public class PyrLkOpticalFlowAlgorithm : IOpticalFlowAlgorithm
    {

        public IEnumerable<FlowModel> CalculateFlow(Image<Gray, byte> prev, Image<Gray, byte> cur)
        {
            var prevFeatures = prev.GoodFeaturesToTrack(500, 0.01d, 0.01d, 10);
            PointF[] currFeatures;
            var criteria = new MCvTermCriteria(300, 0.01);
            byte[] status;
            float[] error;

            OpticalFlow.PyrLK(prev, cur, prevFeatures[0], new Size(21, 21), 3, criteria, out currFeatures, out status, out error);

            var result = new List<FlowModel>();

            var width = prev.Width;
            var height = prev.Height;

            for (int i = 0; i < prevFeatures[0].Length; i++)
            {
                if (status[i] == 1 && currFeatures[i].X <= width && currFeatures[i].Y <= height)
                {
                    if (currFeatures[i].X < 0 || currFeatures[i].Y < 0)
                    {
                        Debug.WriteLine(currFeatures[i].X);
                    }

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
