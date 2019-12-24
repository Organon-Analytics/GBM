using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class DataFrame
    {
        public DataColumnCollection Columns { get; set; }

        private IDictionary<string, List<int>> _rawCategorical;
        private IDictionary<string, List<float>> _rawNumerical;
    }
}
