using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    public class AreaUnderTheCurve
    {
        private readonly double _auc;
        private readonly double _stdError;
        private readonly double _asySignificance;
        private readonly double _asyLowerBound;
        private readonly double _asyUpperBound;
        public AreaUnderTheCurve(double auc, double stdError, double asySignificance, double asyLowerBound, double asyUpperBound)
        {
            _auc = auc;
            _stdError = stdError;
            _asySignificance = asySignificance;
            _asyLowerBound = asyLowerBound;
            _asyUpperBound = asyUpperBound;
        }
        public double Auc { get { return _auc; } }
        public double Roc { get { return _auc; } }
        public double Gini { get { return 2.0 * (_auc - 0.5); } }
        public double StdError { get { return _stdError; } }
        public double AsySignificance { get { return _asySignificance; } }
        public double AsyLowerBound { get { return _asyLowerBound; } }
        public double AsyUpperBound { get { return _asyUpperBound; } }
    }
}
