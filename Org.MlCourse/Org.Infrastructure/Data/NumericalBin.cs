using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class NumericalBin : Bin
    {
        public override int DefaultSupport => throw new NotImplementedException();

        public override BinType BinType => throw new NotImplementedException();

        public override int NumberOfBins => throw new NotImplementedException();

        public override string GetCategoricalValue(int idx)
        {
            throw new NotImplementedException();
        }

        public override IList<string> GetDefaultSet()
        {
            throw new NotImplementedException();
        }

        public override int GetIndex(double dValue)
        {
            throw new NotImplementedException();
        }

        public override int GetIndex(string sValue)
        {
            throw new NotImplementedException();
        }

        public override double GetNumericalThreshold(int idx)
        {
            throw new NotImplementedException();
        }
    }
}
