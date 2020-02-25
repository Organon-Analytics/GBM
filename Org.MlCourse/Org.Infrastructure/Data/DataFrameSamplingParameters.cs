using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class DataFrameSamplingParameters
    {
        public DataFrameSamplingParameters()
        {
            MaxSampleSize = 1000000;
        }
        public int MaxSampleSize { get; set; }
    }
}
