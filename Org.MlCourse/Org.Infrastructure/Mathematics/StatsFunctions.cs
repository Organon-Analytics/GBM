using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public class StatsFunctions
    {
        private readonly IBlas _blas;
        public StatsFunctions(IBlas blas)
        {
            _blas = blas;
        }
        public double[] GetQuantiles(IList<float> data, int numQuantiles)
        {
            if (data == null || data.Count == 0) return null;
            var length = data.Count;
            var copy = new float[length];
            var lengthNaN = _blas.CopyNaN(data, copy, data.Count);
            if (lengthNaN == 0) return null;
            _blas.Sort(copy, 0, lengthNaN);
            var rank = 0.0;
            var delta = 1.0 / numQuantiles;
            var step = 1.0 / lengthNaN;
            var curr = Double.NaN;
            var next = Double.NaN;
            var list = new List<double>();
            for (var i = 0; i < (lengthNaN - 1); i++)
            {
                curr = copy[i];
                next = copy[i + 1];
                rank += step;
                if (next.CompareTo(curr) == 0) continue;
                if (rank > delta)
                {
                    list.Add(curr);
                    rank = 0.0;
                }
                curr = next;
            }
            if (list.Count == 0)
            {
                list.Add(Double.PositiveInfinity);
            }
            else if (list.Count == numQuantiles)
            {
                list[numQuantiles - 1] = Double.PositiveInfinity;
            }
            else
            {
                if (!list.Contains(Double.PositiveInfinity))
                {
                    list.Add(Double.PositiveInfinity);
                }
            }
            return list.ToArray();
        }
    }
}
