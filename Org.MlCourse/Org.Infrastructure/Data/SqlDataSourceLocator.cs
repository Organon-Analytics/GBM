using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class SqlDataSourceLocator : IDataSourceLocator
    {
        private readonly string _connectionName;
        private readonly string _schemaName;
        private readonly string _tableOrView;
        private readonly string _fullName;
        private readonly string _sqlStatement;
        public SqlDataSourceLocator(string connectionName, string schemaName, string tableOrView)
        {
            _connectionName = connectionName;
            _schemaName = schemaName;
            _tableOrView = tableOrView;
            _fullName = String.Format("{0}.{1}", _schemaName, _tableOrView);
            _sqlStatement = String.Empty;
        }

        public SqlDataSourceLocator(string connectionName, string sqlStatement)
        {
            _connectionName = connectionName;
            _schemaName = String.Empty;
            _tableOrView = String.Empty;
            _fullName = String.Empty;
            _sqlStatement = sqlStatement;
        }

        public string ConnectionName { get { return _connectionName; } }
        public DataSourceType DataSourceType
        {
            get
            {
                return String.IsNullOrEmpty(_sqlStatement) ? DataSourceType.SqlTableOrView : DataSourceType.SqlStatement;
            }
        }
        public string FileName { get { throw new InvalidOperationException("FileName is not an applicable property for a database resource"); } }
        public string TableOrViewName { get { return _tableOrView; } }
        public string SchemaName { get { return _schemaName; } }
        public string SqlStatement { get { return _sqlStatement; } }
        public string FullName { get { return _fullName; } }
    }
}
