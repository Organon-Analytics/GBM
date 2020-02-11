using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    [Serializable]
    public class CategoricalIndicator : IndicatorFunction
    {
        private readonly List<int> _discreteSet;
        private readonly bool _doesDefaultExist;
        private readonly bool _defaultIncluded;
        private readonly bool _notIn;
        private readonly int _nullValue = Bin.DefaultIndex;
        private Func<int, bool> _func;

        public CategoricalIndicator(string feature, IList<int> discreteSet, bool doesDefaultExist, bool defaultIncluded, bool notIn)
        {
            _feature = feature;
            _discreteSet = discreteSet.Distinct().ToList();
            _discreteSet.Sort();
            _doesDefaultExist = doesDefaultExist;
            _defaultIncluded = defaultIncluded;
            _notIn = notIn;
            SetFunc();
        }
        public void SetFunc()
        {
            if (_doesDefaultExist)
            {
                if (_defaultIncluded)
                {
                    _func = _notIn ? (Func<int, bool>)FuncTwo : FuncOne;
                }
                else
                {
                    _func = _notIn ? (Func<int, bool>)FuncFour : FuncOne;
                }
            }
            else
            {
                _func = _notIn ? (Func<int, bool>)FuncThree : FuncOne;
            }
        }
        private bool FuncOne(int val)
        {
            return _discreteSet.BinarySearch(val) >= 0;
        }

        private bool FuncTwo(int val)
        {
            return _discreteSet.BinarySearch(val) < 0;
        }

        private bool FuncThree(int val)
        {
            return (val != _nullValue) && _discreteSet.BinarySearch(val) < 0;
        }

        private bool FuncFour(int val)
        {
            return (val == _nullValue) || _discreteSet.BinarySearch(val) < 0;
        }

        public override bool Contains(string val)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(int val)
        {
            return _func(val);
        }

        public override bool Contains(double val)
        {
            throw new NotImplementedException();
        }

        public string ToSqlX(Bin bin)
        {
            var catBin = bin as CategoricalBin;
            if (catBin == null)
            {
                throw new ArgumentException(
                    "The bin object as input to CategoricalIndicator.ToSql(.) method should be of type CategoricalBin");
            }
            var defaultIdx = Bin.DefaultIndex;
            var containsDefault = _discreteSet.Contains(defaultIdx);
            if (containsDefault)
            {
                Console.WriteLine("Hello Categorical");
                if (_discreteSet.Count == 1)
                {
                    return String.Format("({0} IS NULL)", Feature);
                }
                else
                {
                    var defaultExcluded = _discreteSet.Except(new List<int> { defaultIdx }).ToList();
                    var concat = ConcatenateList(defaultExcluded, catBin);
                    return _notIn
                        ? String.Format("(({0} IS NOT NULL) AND {0} NOT IN ({1}))", Feature, concat)
                        : String.Format("(({0} IS NULL) OR {0} IN ({1}))", Feature, concat);
                }
            }
            else
            {
                var concat = ConcatenateList(_discreteSet, catBin);
                return _notIn
                    ? String.Format("{0} NOT IN ({1})", Feature, concat)
                    : String.Format("{0} IN ({1})", Feature, concat);
            }
        }

        public override string ToSql(Bin bin)
        {
            var catBin = bin as CategoricalBin;
            if (catBin == null)
            {
                throw new ArgumentException(
                    "The bin object as input to CategoricalIndicator.ToSql(.) method should be of type CategoricalBin");
            }
            var defaultIdx = Bin.DefaultIndex;
            var defaultExcluded = _discreteSet.Except(new List<int> { defaultIdx }).ToList();
            var withoutDefault = ConcatenateList(defaultExcluded, catBin);
            if (_doesDefaultExist)
            {
                if (_defaultIncluded)
                {
                    if (_notIn)
                    {
                        return (defaultExcluded.Count == 0)
                            ? String.Format("({0} IS NOT NULL)", Feature)
                            : String.Format("(({0} IS NOT NULL) AND {0} NOT IN ({1}))", Feature, withoutDefault);
                    }
                    else
                    {
                        return (defaultExcluded.Count == 0)
                            ? String.Format("({0} IS NULL)", Feature)
                            : String.Format("(({0} IS NULL) OR {0} IN ({1}))", Feature, withoutDefault);
                    }
                }
                else
                {
                    if (_notIn)
                    {
                        return (defaultExcluded.Count == 0)
                            ? String.Format("({0} IS NULL)", Feature)
                            : String.Format("(({0} IS NULL) OR {0} NOT IN ({1}))", Feature, withoutDefault);
                    }
                    else
                    {
                        return (defaultExcluded.Count == 0)
                            ? String.Format("({0} IS NOT NULL)", Feature)
                            : String.Format("(({0} IS NOT NULL) AND {0} IN ({1}))", Feature, withoutDefault);
                    }
                }
            }
            else
            {
                if (_notIn)
                {
                    return String.Format("(({0} IS NOT NULL) AND {0} NOT IN ({1}))", Feature, withoutDefault);
                }
                else
                {
                    return String.Format("({0} IN ({1}))", Feature, withoutDefault);
                }
            }
        }

        private string ConcatenateList(IList<int> indices, CategoricalBin bin)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < indices.Count; i++)
            {
                var idx = indices[i];
                var rawString = bin.GetCategoricalValue(idx);
                var s = rawString.Contains("'") ? rawString.Replace("'", "''") : rawString;
                builder.Append(String.Format("'{0}'", s));
                if (i < (indices.Count - 1))
                {
                    builder.Append(',');
                }
            }
            return builder.ToString();
        }

        public override IndicatorFunction Clone()
        {
            var indicator = new CategoricalIndicator(_feature, _discreteSet.ToList(), _doesDefaultExist,
                _defaultIncluded, _notIn);
            indicator.SetFunc();
            return indicator;
        }
    }
}
