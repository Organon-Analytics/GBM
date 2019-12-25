using Org.Infrastructure.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class DataFrame
    {
        private IDictionary<string, Bin> _binCollection;
        private Bijection<string, int> _columnOrder;
        public DataColumnCollection _columnCollection;

        private IDictionary<string, List<int>> _rawCategorical;
        private IDictionary<string, List<float>> _rawNumerical;

        private IDictionary<string, Action<string, string>> _actions;
        public void Initialize(DataColumnCollection collection, int capacity)
        {
            _columnCollection = collection;
            _binCollection = new Dictionary<string, Bin>();
            _rawCategorical = new Dictionary<string, List<int>>();
            _rawNumerical = new Dictionary<string, List<float>>();

            _actions = new Dictionary<string, Action<string, string>>();
            foreach (var column in _columnCollection.Values)
            {
                var name = column.Name;
                //var order = column.Order;
                //_columnOrder.Add(name, order);
                if (column.MeasurementType == ColumnMeasurementType.Categorical)
                {
                    _rawCategorical.Add(name, new List<int>(capacity));
                    _binCollection.Add(column.Name, new CategoricalBin(column.MissingNominalValues));
                    _actions.Add(name, AddCategorical);
                }
                else if (column.MeasurementType == ColumnMeasurementType.Numerical)
                {
                    _rawNumerical.Add(name, new List<float>(capacity));
                    _actions.Add(name, AddFloat);
                }
            }
        }

        public void Add(int order, string s)
        {
            var name = _columnOrder[order];
            _actions[name](name, s);
        }

        private void AddCategorical(string name, string s)
        {
            //var bin = (CategoricalBin)_binCollection[name];
            //bin.Add(s);
            //var idx = bin.GetIndex(s);
            //_rawCategorical[name].Add(idx);
        }

        private void AddFloat(string name, string s)
        {
            //var f = Single.NaN;
            //var flag = Single.TryParse(s, out f);
            //_rawNumerical[name].Add(flag ? f : Single.NaN);
        }
    }
}
