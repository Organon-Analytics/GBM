using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public class UnivariateEstimate
    {
        private readonly double _estimate;
        private readonly double _alpha;
        private readonly double _lcl;
        private readonly double _ucl;
        public UnivariateEstimate(double estimate, double alpha, double lowerConfidenceLevel, double upperConfidenceLevel)
        {
            _estimate = estimate;
            _alpha = alpha;
            _lcl = lowerConfidenceLevel;
            _ucl = upperConfidenceLevel;
        }
        public double Estimate { get { return _estimate; } }
        public double Alpha { get { return _alpha; } }
        public double LowerConfidenceLevel { get { return _lcl; } }
        public double UpperConfidenceLevel { get { return _ucl; } }
    }
}
