using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public class BivariateMeanCovariance
    {
        private readonly double _m1;
        private readonly double _m2;
        private readonly double _s1;
        private readonly double _s2;
        private readonly double _s12;

        public BivariateMeanCovariance(double mu1, double mu2, double s1, double s2, double s12)
        {
            _m1 = mu1;
            _m2 = mu2;
            _s1 = s1;
            _s2 = s2;
            _s12 = s12;
        }

        private bool Corrupt()
        {
            var c1 = Double.IsNaN(_s1) || (_s1 <= Double.Epsilon);
            var c2 = Double.IsNaN(_s2) || (_s2 <= Double.Epsilon);
            var c12 = Double.IsNaN(_s12) || (Math.Abs(_s12) <= Double.Epsilon);
            return c1 || c2 || c12;
        }

        public double MeanFirst { get { return _m1; } }
        public double MeanSecond { get { return _m2; } }
        public double SigmaFirst { get { return _s1; } }
        public double SigmaSecond { get { return _s2; } }
        public double Covariance { get { return _s12; } }
        public double Correlation
        {
            get { return Corrupt() ? Double.NaN : _s12 / (_s1 * _s2); }
        }
    }
}
