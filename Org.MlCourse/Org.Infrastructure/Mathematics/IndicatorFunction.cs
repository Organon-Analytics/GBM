using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    [Serializable]
    //TODO: Move to FrameworkLibrary
    public abstract class IndicatorFunction
    {
        protected string _feature;
        public string Feature { get { return _feature; } }
        public abstract bool Contains(string val);
        public abstract bool Contains(int val);
        public abstract bool Contains(double val);
        public abstract string ToSql(Bin bin);
        public abstract IndicatorFunction Clone();
    }
}
