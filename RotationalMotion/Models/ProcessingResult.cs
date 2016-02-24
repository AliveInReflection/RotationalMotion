﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace RotationalMotion.Models
{
    public class ProcessingResult
    {
        public Bitmap Previous { get; set; }
        public Bitmap Current { get; set; }
        public Matrix Rotation { get; set; }
    }
}
