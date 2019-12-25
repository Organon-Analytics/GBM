using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Collections
{
    public class Bijection<TKey, TVal>
    {
        private IDictionary<TKey, TVal> _forward;
        private IDictionary<TVal, TKey> _reverse;

        public Bijection()
        {
            _forward = new Dictionary<TKey, TVal>();
            _reverse = new Dictionary<TVal, TKey>();
        }

        public void Add(TKey key, TVal val)
        {
            _forward.Add(key, val);
            _reverse.Add(val, key);
        }

        public TVal this[TKey key]
        {
            get { return _forward[key]; }
        }
        public TKey this[TVal val]
        {
            get { return _reverse[val]; }
        }
    }
}
