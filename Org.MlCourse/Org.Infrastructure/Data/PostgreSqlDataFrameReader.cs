using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class PostgreSqlDataFrameReader : DataFrameReader
    {
        public override DataFrame Read(IDataSourceLocator source, DataFrameSamplingParameters samplingParameters)
        {
            throw new NotImplementedException();
        }

        public override DataFrame Read(IDataSourceLocator source, IList<string> excludedColumnList, DataFrameSamplingParameters samplingParameters)
        {
            throw new NotImplementedException();
        }
    }
}
