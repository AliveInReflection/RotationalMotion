using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotationalMotion.Models
{
    public class OpticalFlowModel
    { 
        public IEnumerable<FlowModel> Flow { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
