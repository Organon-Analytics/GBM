using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class DataColumn
    {
        public DataColumn(string name, int order)
        {
            Name = name;
            Order = order;
            NativeType = ColumnNativeType.Other;
            MeasurementType = ColumnMeasurementType.Other;
            MissingNominalValues = new List<string> { String.Empty };
            MissingFloatValues = new List<float> { Single.NaN };
            MissingDoubleValues = new List<double> { Double.NaN };
        }
        public string Name { get; set; }
        public int Order { get; set; }
        public ColumnNativeType NativeType { get; set; }
        public ColumnMeasurementType MeasurementType { get; set; }
        public IList<string> MissingNominalValues { get; set; }
        public IList<float> MissingFloatValues { get; set; }
        public IList<double> MissingDoubleValues { get; set; }


        public static ColumnNativeType GetNativeType(object obj)
        {
            var s = obj as String;
            if (s == null) return ColumnNativeType.Other;
            var intOutput = Int32.MinValue;
            var success = Int32.TryParse(s, out intOutput);
            if (success)
            {
                return ColumnNativeType.Integer;
            }
            var fpOutput = Double.NaN;
            success = Double.TryParse(s, out fpOutput);
            if (success)
            {
                return ColumnNativeType.FloatingPoint;
            }
            var dtOutput = DateTime.Now;
            success = DateTime.TryParse(s, out dtOutput);
            if (success)
            {
                return ColumnNativeType.DateTime;
            }
            return ColumnNativeType.String;
        }
        public static ColumnMeasurementType GetDefaultMeasurementType(ColumnNativeType nativeType)
        {
            if (nativeType == ColumnNativeType.Integer || nativeType == ColumnNativeType.FloatingPoint)
            {
                return ColumnMeasurementType.Numerical;
            }
            else if (nativeType == ColumnNativeType.String || nativeType == ColumnNativeType.Text)
            {
                return ColumnMeasurementType.Categorical;
            }
            else
            {
                return ColumnMeasurementType.Other;
            }
        }
    }
}
