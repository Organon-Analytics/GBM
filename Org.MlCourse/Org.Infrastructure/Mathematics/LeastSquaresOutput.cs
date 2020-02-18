using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public class LeastSquaresOutput
    {
        public double SampleWeight { get; set; }
        public double RSquared { get; set; }
        public double ResidualSumOfSquares { get; set; }
        public double MeanSquareError { get; set; }
    }
}
