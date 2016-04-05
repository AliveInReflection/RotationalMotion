using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace RotationalMotion.Models
{
    public class ProcessingResult
    {
        public Bitmap Frame { get; set; }

        public AngularPositionModel AngularPosition { get; set; }
    }
}
