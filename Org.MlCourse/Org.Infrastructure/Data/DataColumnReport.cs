using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class DataColumnReport
    {
        public DataColumnCollection ColumnCollection { get; set; }
        public IList<string> InvalidColumns { get; set; }
        public int ScannedNumOfLines { get; set; }
    }
}
