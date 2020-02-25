using Org.Infrastructure.Collections;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class DataFrame
    {
        public DataColumnCollection _columnCollection;
        private Bijection<string, int> _columnOrder;
        
        private IDictionary<string, List<int>> _rawCategorical;
        private IDictionary<string, List<float>> _rawNumerical;
        private IDictionary<string, int[]> _integerFrame;
        private int _rowCount;

        private IDictionary<string, Action<string, string>> _actions;
        private Dictionary<string, Bin> _binCollection;

        private Random _rng;
        private IBlas _blas;

        private int[] _indices;
        private int[] _trainingIndices;
        private int[] _randomTrainingIndices;
        private int[] _validationIndices;

        public DataFrame()
        {
            _blas = new DotNetBlas();
            _rng = new Random();
        }

        public int[] GetIntegerArray(string columnName)
        {
            return _integerFrame[columnName];
        }

        //
        public float[] GetDoubleRawArray(string columnName)
        {
            return _rawNumerical[columnName].ToArray();
        }
        public int[] GetTrainingIndices()
        {
            return _trainingIndices;
        }

        public int[] GetRandomTrainingIndices()
        {
            return _randomTrainingIndices;
        }

        public int[] GetValidationIndices()
        {
            return _validationIndices;
        }

        public int[] GetIndices()
        {
            return _indices;
        }

        public Dictionary<string, Bin> GetBinCollection()
        {
            return _binCollection;
        }

        public Bin GetBin(string column)
        {
            return _binCollection[column];
        }
        public void Initialize(DataColumnCollection collection, int capacity)
        {
            _columnCollection = collection;
            _binCollection = new Dictionary<string, Bin>();
            _columnOrder = new Bijection<string, int>();
            _rawCategorical = new Dictionary<string, List<int>>();
            _rawNumerical = new Dictionary<string, List<float>>();

            _actions = new Dictionary<string, Action<string, string>>();
            foreach (var column in _columnCollection.Values)
            {
                var name = column.Name;
                var order = column.Order;
                _columnOrder.Add(name, order);
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
            var bin = (CategoricalBin)_binCollection[name];
            bin.Add(s);
            var idx = bin.GetIndex(s);
            _rawCategorical[name].Add(idx);
        }

        private void AddFloat(string name, string s)
        {
            var f = Single.NaN;
            var flag = Single.TryParse(s, out f);
            _rawNumerical[name].Add(flag ? f : Single.NaN);
        }

        public void FinalizeAfterRead()
        {
            _rowCount = 1;
            foreach (var item in _rawCategorical)
            {
                _rowCount = item.Value.Count;
            }
            foreach (var item in _rawNumerical)
            {
                _rowCount = item.Value.Count;
            }
        }
        public void SetBlas(IBlas blas)
        {
            _blas = blas;
        }

        public void CreateBins(int maxBins)
        {
            _indices = new int[_rowCount];
            for (var i = 0; i < _indices.Length; i++)
            {
                _indices[i] = i;
            }

            _integerFrame = new Dictionary<string, int[]>();
            var categoricalKeys = _rawCategorical.Keys.ToList();
            foreach (var column in categoricalKeys)
            {
                var list = _rawCategorical[column];
                _integerFrame.Add(column, list.ToArray());
                _rawCategorical.Remove(column);
            }

            var helper = new StatsFunctions(_blas);
            foreach (var item in _rawNumerical)
            {
                var name = item.Key;
                var src = item.Value;
                var thresholds = helper.GetQuantiles(src, maxBins);
                if (thresholds == null) continue;
                var bin = new NumericalBin(thresholds);
                var dest = new int[src.Count];
                for (int i = 0; i < src.Count; i++)
                {
                    dest[i] = bin.GetIndex(src[i]);
                }
                _binCollection.Add(name, bin);
                _integerFrame.Add(name, dest);
            }
        }

        public void Partition(double trainingRatio)
        {
            var length = _indices.Length;
            var tList = new List<int>(length);
            var vList = new List<int>(length);
            if (trainingRatio <= 0.0 || trainingRatio >= 1.0)
            {
                for (var i = 0; i < length; i++)
                {
                    tList.Add(i);
                }
                _trainingIndices = tList.ToArray();
                _validationIndices = null;
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    if (_rng.NextDouble() < trainingRatio)
                    {
                        tList.Add(i);
                    }
                    else
                    {
                        vList.Add(i);
                    }
                }
                _trainingIndices = tList.ToArray();
                _validationIndices = vList.ToArray();
            }
        }

        public void RandomizeTrainingIndices(double rowSamplingRate)
        {
            var list = new List<int>(_trainingIndices.Length);
            list.AddRange(_trainingIndices.Where(idx => _rng.NextDouble() < rowSamplingRate));
            _randomTrainingIndices = list.ToArray();
        }

        public IList<string> GetRandomInputList(IList<string> columnList, double columnSamplingRate)
        {
            return columnList.Where(column => _rng.NextDouble() < columnSamplingRate).ToList();
        }

        public int GetRowCount()
        {
            return _rowCount;
        }

        public IList<string> GetConstantColumns()
        {
            var list = new List<string>();
            foreach (var pair in _integerFrame)
            {
                var columnName = pair.Key;
                var array = pair.Value;
                var isConstant = _blas.IsConstant(array);
                if (!isConstant) continue;
                list.Add(columnName);
            }
            return list;
        }

        public bool IsConstant(string columnName)
        {
            if (!_integerFrame.ContainsKey(columnName))
            {
                throw new InvalidOperationException(
                    String.Format("There is no data in the binned data with the column name {0}", columnName));
            }
            return _blas.IsConstant(_integerFrame[columnName]);
        }

        public string[] GetEligibleInputColumns(IList<string> inColumns, IList<string> excludedColumnList)
        {
            var all = GetBinnedColumnList();
            var allMinusTarget = all.Except(excludedColumnList).ToList();
            string[] temp;
            if (inColumns != null && inColumns.Count > 0)
            {
                temp = allMinusTarget.Intersect(inColumns).ToArray();
            }
            else
            {
                temp = allMinusTarget.ToArray();
            }
            var constantColumns = GetConstantColumns();
            var final = temp.Except(constantColumns).ToArray();
            return final;
        }

        public IList<string> GetBinnedColumnList()
        {
            return _integerFrame.Keys.ToList();
        }
    }
}
