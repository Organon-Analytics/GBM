using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public enum GbmTreeNodeType
    {
        Root,
        Interim,
        Leaf,
        Orphan
    }
}
