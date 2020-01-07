using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class NumericalBin : Bin
    {
        private readonly double[] _thresholds;
        private readonly int _length;
        private readonly int _numBin;
        private readonly int _defaultSupport;
        public NumericalBin(double[] thresholds)
        {
            if (thresholds == null || thresholds.Length == 0)
            {
                throw new ArgumentException("Threshold array can not be null or empty");
            }
            if (thresholds[thresholds.Length - 1].CompareTo(Double.PositiveInfinity) != 0)
            {
                throw new InvalidOperationException("The last threshold value for a numerical bin must be Positive Infinity");
            }

            _thresholds = thresholds;
            _length = _thresholds.Length;
            Array.Sort(_thresholds);
            _numBin = 1 + _length;
            _defaultSupport = 0;
        }

        public double[] Thresholds { get { return _thresholds; } }

        public override int DefaultSupport
        {
            get { return _defaultSupport; }
        }

        public override BinType BinType
        {
            get { return BinType.Numerical; }
        }

        public override int NumberOfBins
        {
            get { return _numBin; }
        }

        public override int GetIndex(double dValue)
        {
            if (Double.IsNaN(dValue)) return DefaultIdx;
            for (var i = 0; i < _length; i++)
            {
                if (dValue <= _thresholds[i])
                {
                    return 1 + i;
                }
            }
            return _length;
        }

        public override int GetIndex(string sValue)
        {
            throw new InvalidOperationException("A categorical bin does not implement GetIndex(string sValue) method");
        }

        public override IList<string> GetDefaultSet()
        {
            throw new InvalidOperationException("GetDefaultSet(.) method is not applicable for NumericalBin");
        }

        public override string GetCategoricalValue(int idx)
        {
            throw new InvalidOperationException("GetCategoricalValue(.) method is not applicable for NumericalBin");
        }

        public override double GetNumericalThreshold(int idx)
        {
            return _thresholds[idx - 1];
        }
    }
}
