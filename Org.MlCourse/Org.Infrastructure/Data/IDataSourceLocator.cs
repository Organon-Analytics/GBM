using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public interface IDataSourceLocator
    {
        DataSourceType DataSourceType { get; }
        string FileName { get; }
        string ConnectionName { get;  }
        string SchemaName { get; }
        string SqlStatement { get; }
        string TableOrViewName { get;  }
        string FullName { get; }
    }
}
