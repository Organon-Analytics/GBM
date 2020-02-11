using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    [Serializable]
    public class GbmHyperParameterSearchResult
    {
        public GbmHyperParameters HyperParameters { get; set; }
        public double TrainingLoss { get; set; }
        public double ValidationLoss { get; set; }
        public double TestLoss { get; set; }
        public double TrainingAuc { get; set; }
        public double ValidationAuc { get; set; }
        public double TestAuc { get; set; }
        public double TrainingR2 { get; set; }
        public double ValidationR2 { get; set; }
        public double TestR2 { get; set; }
        public double RuntimeDurationInSeconds { get; set; }
    }
}
