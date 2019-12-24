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
    }
}