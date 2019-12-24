using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model
{
    public class ModellingDataSettings
    {
        public IDataSourceLocator DevelopmentDataSource { get; set; }
        public DataFrameSamplingParameters DevelopmentFrameSamplingParameters { get; set; }
        public IDataSourceLocator TestDataSource { get; set; }
        public DataFrameSamplingParameters TestFrameSamplingParameters { get; set; }
        public IList<string> IncludedColumns { get; set; }
        public IList<string> ExcludedColumns { get; set; }
        public string TargetColumnName { get; set; }
        public string PositiveCategory { get; set; }
        public string NegativeCategory { get; set; }
        public double TrainingRatio { get; set; }
    }
}
