using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public abstract class Bin
    {
        protected static int DefaultIdx = 0;
        public abstract int DefaultSupport { get; }
        public abstract BinType BinType { get; }
        public abstract int NumberOfBins { get; }
        public static int DefaultIndex { get { return DefaultIdx; } }
        public abstract int GetIndex(double dValue);
        public abstract int GetIndex(string sValue);
        public abstract IList<string> GetDefaultSet();
        public abstract string GetCategoricalValue(int idx);
        public abstract double GetNumericalThreshold(int idx);
    }
}
