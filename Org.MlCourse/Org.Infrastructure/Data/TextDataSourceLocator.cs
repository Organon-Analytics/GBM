using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class TextDataSourceLocator : IDataSourceLocator
    {
        private readonly string _fileName;
        public TextDataSourceLocator(string fileName)
        {
            _fileName = fileName;
        }
        public bool ContainsHeaderWithColumnNames { get; set; }
        public char Delimiter { get; set; }
        public int NumLinesForTypeInference { get; set; }
        public DataSourceType DataSourceType { get { return DataSourceType.TextFile; } }
        public string FileName { get { return _fileName; } }
        public string TableOrViewName { get { throw new InvalidOperationException("TableName is not an applicable property for a file"); } }
        public string SchemaName { get { throw new InvalidOperationException("SchemaName is not an applicable property for a file"); } }
        public string SqlStatement { get { throw new InvalidOperationException("SqlStatement is not an applicable property for a file"); } }
        public string FullName { get { return _fileName; } }
        public string ConnectionName
        {
            get { throw new InvalidOperationException("TableName is not an applicable property for a file"); }
        }
    }
}
