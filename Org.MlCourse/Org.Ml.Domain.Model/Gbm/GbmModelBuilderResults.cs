using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class GbmModelBuilderResults
    {
        public GbmModelDetail GbmModelDetail { get; set; }
        public IList<GbmHyperParameterSearchResult> ParameterSearchResults { get; set; }
        public double[] Predictions { get; set; }
    }
}
