using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    [Serializable]
    public struct LossPerIteration
    {
        public double TrainingLoss { get; set; }
        public double ValidationLoss { get; set; }
    }
}
