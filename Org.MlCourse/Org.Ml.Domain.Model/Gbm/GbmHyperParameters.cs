using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    [Serializable]
    public class GbmHyperParameters
    {
        public double LearningRate { get; set; }
        public double RowSamplingRate { get; set; }
        public double ColumnSamplingRate { get; set; }
        public double L3Penalty { get; set; }
        public double L1Penalty { get; set; }
        public int MaxTreeLeaves { get; set; }
        public int MaxCategory { get; set; }
        public int MaxIterations { get; set; }
    }
}
