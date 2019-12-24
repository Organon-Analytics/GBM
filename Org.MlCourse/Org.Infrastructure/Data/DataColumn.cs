using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class DataColumn
    {
        public DataColumn(string name)
        {
            Name = name;
            NativeType = ColumnNativeType.Other;
            MeasurementType = ColumnMeasurementType.Other;
            MissingCategoricalValues = new List<string> { String.Empty };
            MissingFloatValues = new List<float> { Single.NaN };
            MissingDoubleValues = new List<double> { Double.NaN };
        }
        public string Name { get; set; }
        public ColumnNativeType NativeType { get; set; }
        public ColumnMeasurementType MeasurementType { get; set; }
        public IList<string> MissingCategoricalValues { get; set; }
        public IList<float> MissingFloatValues { get; set; }
        public IList<double> MissingDoubleValues { get; set; }
    }
}
