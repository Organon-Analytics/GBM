using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public abstract class DataFrameReader
    {
        public abstract DataFrame Read(IDataSourceLocator source, DataFrameSamplingParameters samplingParameters);
        public abstract DataFrame Read(IDataSourceLocator source, IList<string> excludedColumnList,
            DataFrameSamplingParameters samplingParameters);

        protected string PrepareSql(IDataSourceLocator source, IList<string> columnNames)
        {
            var builder = new StringBuilder();
            builder.AppendLine("SELECT");
            if (columnNames == null || columnNames.Count == 0)
            {
                builder.AppendLine("*");
            }
            else
            {
                for (int i = 0; i < columnNames.Count; i++)
                {
                    builder.Append(columnNames[i]);
                    if (i < (columnNames.Count - 1))
                    {
                        builder.AppendLine(",");
                    }
                    else
                    {
                        builder.AppendLine();
                    }
                }
            }
            builder.AppendLine("FROM");
            switch (source.DataSourceType)
            {
                case DataSourceType.SqlTableOrView:
                    builder.AppendLine(source.FullName);
                    break;
                case DataSourceType.SqlStatement:
                    builder.AppendLine("(");
                    builder.AppendLine(source.SqlStatement);
                    builder.AppendLine(") T1");
                    break;
            }
            return builder.ToString();
        }

        public static TextDataFrameReader GetDataFrameReader(IDataSourceLocator source)
        {
            if(source.DataSourceType == DataSourceType.TextFile)
            {
                return new TextDataFrameReader();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
