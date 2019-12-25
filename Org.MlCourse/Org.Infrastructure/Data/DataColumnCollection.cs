using System;
using System.Collections.Generic;

namespace Org.Infrastructure.Data
{
    public class DataColumnCollection : Dictionary<string, DataColumn>
    {
        public DataColumnCollection Filter(IList<string> columnNameList)
        {
            var output = new DataColumnCollection();
            foreach (var name in columnNameList)
            {
                if (columnNameList.Contains(name) && this.ContainsKey(name))
                {
                    output.Add(name, this[name]);
                }
            }
            return output;
        }

        public int[] GetValidIndices()
        {
            var list = new List<int>();
            foreach (var item in this)
            {
                var mType = item.Value.MeasurementType;
                if (mType == ColumnMeasurementType.Categorical || mType == ColumnMeasurementType.Numerical)
                {
                    list.Add(item.Value.Order);
                }
            }
            list.Sort();
            return list.ToArray();
        }
    }
}