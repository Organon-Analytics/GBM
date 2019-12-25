using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Org.Infrastructure.Data
{
    public class CategoricalBin : Bin
    {
        private readonly List<string> _defaultSet;
        private int _defaultSupport;
        private readonly IDictionary<string, int> _frequencies;
        private readonly IDictionary<string, int> _index;
        private readonly IDictionary<int, string> _map;

        public CategoricalBin(IEnumerable<string> defaultSet)
        {
            _defaultSet = defaultSet.ToList();
            if (!_defaultSet.Contains(String.Empty))
            {
                _defaultSet.Add(String.Empty);
            }
            _defaultSet.Sort();
            _defaultSupport = 0;
            _frequencies = new Dictionary<string, int>();
            _index = new Dictionary<string, int>();
            foreach (var item in _defaultSet)
            {
                _frequencies.Add(item, 0);
                _index.Add(item, DefaultIndex);
            }
            _map = new Dictionary<int, string>();
        }

        public override int DefaultSupport
        {
            get { return _defaultSupport; }
        }

        public override BinType BinType
        {
            get { return BinType.Categorical; }
        }

        public override int NumberOfBins
        {
            get { return 1 + _map.Count; }
        }

        public override int GetIndex(double dValue)
        {
            throw new InvalidOperationException("A categorical bin does not implement GetIndex(double dValue) method");
        }

        public override int GetIndex(string sValue)
        {
            return _index.ContainsKey(sValue) ? _index[sValue] : DefaultIdx;
        }

        public void Add(string key)
        {
            var idx = _defaultSet.BinarySearch(key);
            if (idx >= 0)
            {
                _defaultSupport++;
                _frequencies[key]++;
            }
            else if (_frequencies.ContainsKey(key))
            {
                _frequencies[key]++;
            }
            else
            {
                var pos = _map.Count + 1;
                _frequencies.Add(key, 1);
                _index.Add(key, pos);
                _map.Add(pos, key);
            }
        }

        public void Add(string key, int frequency)
        {
            var idx = _defaultSet.BinarySearch(key);
            if (idx >= 0)
            {
                _defaultSupport += frequency;
                _frequencies[key] += frequency;
            }
            else if (_frequencies.ContainsKey(key))
            {
                _frequencies[key] += frequency;
            }
            else
            {
                var pos = _map.Count + 1;
                _frequencies.Add(key, frequency);
                _index.Add(key, pos);
                _map.Add(pos, key);
            }
        }

        public string GetValue(int index)
        {
            return _map[index];
        }

        public long GetFrequency(string key)
        {
            return _frequencies[key];
        }

        public override IList<string> GetDefaultSet()
        {
            return _defaultSet;
        }

        public override string GetCategoricalValue(int idx)
        {
            return _map[idx];
        }

        public override double GetNumericalThreshold(int idx)
        {
            throw new InvalidOperationException("GetNumericalThreshold(.) method is not applicable for CategoricalBin");
        }
    }
}
