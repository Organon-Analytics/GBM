using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Mathematics
{
    [Serializable]
    public class NumericalIndicator : IndicatorFunction
    {
        private readonly int _integerThreshold;
        private readonly ComparisonOperator _op;
        private readonly bool _doesDefaultExist;
        private readonly bool _defaultOnLeft;
        [NonSerialized]
        private Func<int, bool> _func;
        public NumericalIndicator(string feature, bool doesDefaultExist, bool defaultOnLeft, int integerThreshold, ComparisonOperator op)
        {
            _feature = feature;
            _doesDefaultExist = doesDefaultExist;
            _defaultOnLeft = defaultOnLeft;
            _integerThreshold = integerThreshold;
            _op = op;
            SetFunc();
        }

        public void SetFunc()
        {
            var defaultIdx = Bin.DefaultIndex;
            if (_doesDefaultExist)
            {
                if (_defaultOnLeft)
                {
                    switch (_op)
                    {
                        case ComparisonOperator.Greater:
                            _func = (d => (d != defaultIdx) && (d > _integerThreshold));
                            break;
                        case ComparisonOperator.GreaterOrEqual:
                            _func = (d => (d != defaultIdx) && (d >= _integerThreshold));
                            break;
                        case ComparisonOperator.Lesser:
                            _func = (d => (d == defaultIdx) || (d < _integerThreshold));
                            break;
                        case ComparisonOperator.LesserOrEqual:
                            _func = (d => (d == defaultIdx) || (d <= _integerThreshold));
                            break;
                    }
                }
                else
                {
                    switch (_op)
                    {
                        case ComparisonOperator.Greater:
                            _func = (d => (d == defaultIdx) || (d > _integerThreshold));
                            break;
                        case ComparisonOperator.GreaterOrEqual:
                            _func = (d => (d == defaultIdx) || (d >= _integerThreshold));
                            break;
                        case ComparisonOperator.Lesser:
                            _func = (d => (d != defaultIdx) && (d < _integerThreshold));
                            break;
                        case ComparisonOperator.LesserOrEqual:
                            _func = (d => (d != defaultIdx) && (d <= _integerThreshold));
                            break;
                        default:
                            throw new InvalidOperationException(String.Format("Comparison operator {0} is not valid", _op));
                    }
                }

            }
            else
            {
                switch (_op)
                {
                    case ComparisonOperator.Greater:
                        _func = (d => (d != defaultIdx) && (d > _integerThreshold));
                        break;
                    case ComparisonOperator.GreaterOrEqual:
                        _func = (d => (d != defaultIdx) && (d >= _integerThreshold));
                        break;
                    case ComparisonOperator.Lesser:
                        _func = (d => (d != defaultIdx) && (d < _integerThreshold));
                        break;
                    case ComparisonOperator.LesserOrEqual:
                        _func = (d => (d != defaultIdx) && (d <= _integerThreshold));
                        break;
                }
            }
        }
        //TODO: There is explicit zero below
        public Func<int, bool> GetFunc()
        {
            var defaultIdx = Bin.DefaultIndex;
            if (_doesDefaultExist)
            {
                if (_defaultOnLeft)
                {
                    switch (_op)
                    {
                        case ComparisonOperator.Greater:
                            return (d => (d != defaultIdx) && (d > _integerThreshold));
                        case ComparisonOperator.GreaterOrEqual:
                            return (d => (d != defaultIdx) && (d >= _integerThreshold));
                        case ComparisonOperator.Lesser:
                            return (d => (d == defaultIdx) || (d < _integerThreshold));
                        case ComparisonOperator.LesserOrEqual:
                            return (d => (d == defaultIdx) || (d <= _integerThreshold));
                        default:
                            throw new InvalidOperationException(String.Format("Comparison operator {0} is not valid", _op));
                    }
                }
                else
                {
                    switch (_op)
                    {
                        case ComparisonOperator.Greater:
                            return (d => (d == defaultIdx) || (d > _integerThreshold));
                        case ComparisonOperator.GreaterOrEqual:
                            return (d => (d == defaultIdx) || (d >= _integerThreshold));
                        case ComparisonOperator.Lesser:
                            return (d => (d != defaultIdx) && (d < _integerThreshold));
                        case ComparisonOperator.LesserOrEqual:
                            return (d => (d != defaultIdx) && (d <= _integerThreshold));
                        default:
                            throw new InvalidOperationException(String.Format("Comparison operator {0} is not valid", _op));
                    }
                }

            }
            else
            {
                switch (_op)
                {
                    case ComparisonOperator.Greater:
                        return (d => (d != defaultIdx) && (d > _integerThreshold));
                    case ComparisonOperator.GreaterOrEqual:
                        return (d => (d != defaultIdx) && (d >= _integerThreshold));
                    case ComparisonOperator.Lesser:
                        return (d => (d != defaultIdx) && (d < _integerThreshold));
                    case ComparisonOperator.LesserOrEqual:
                        return (d => (d != defaultIdx) && (d <= _integerThreshold));
                    default:
                        throw new InvalidOperationException(String.Format("Comparison operator {0} is not valid", _op));
                }
            }
        }

        public override bool Contains(string val)
        {
            throw new InvalidOperationException("String argument is not accepted into NumericalIndicator function");
        }

        public override bool Contains(int val)
        {
            return _func(val);
        }

        public override bool Contains(double val)
        {
            throw new NotImplementedException();
        }


        public override string ToSql(Bin bin)
        {
            var numBin = bin as NumericalBin;
            if (numBin == null)
            {
                throw new ArgumentException(
                    "The bin object as input to NumericalBin.ToSql() method should be of type NumericalBin");
            }
            var defaultIdx = Bin.DefaultIndex;
            //if(_integerThreshold == defaultIdx || _integerThreshold == numBin.Thresholds.Length )
            var doubleThreshold = bin.GetNumericalThreshold(_integerThreshold);
            if (doubleThreshold.CompareTo(Double.PositiveInfinity) >= 0)
                throw new InvalidOperationException("Threshold could not be equal to Positive Infinity");
            if (!_doesDefaultExist)
            {
                if (Double.IsNaN(doubleThreshold))
                {
                    throw new InvalidOperationException("Threshold for Numerical Indicator can not be null when no default exists");
                }
                switch (_op)
                {
                    case ComparisonOperator.Greater:
                        return String.Format("({0} > {1})", Feature, doubleThreshold);
                    case ComparisonOperator.GreaterOrEqual:
                        return String.Format("({0} >= {1})", Feature, doubleThreshold);
                    case ComparisonOperator.Lesser:
                        return String.Format("({0} < {1})", Feature, doubleThreshold);
                    case ComparisonOperator.LesserOrEqual:
                        return String.Format("({0} <= {1})", Feature, doubleThreshold);
                    default:
                        throw new InvalidOperationException(String.Format("Comparison operator {0} is not valid", _op));
                }
            }
            if (Double.IsNaN(_integerThreshold))
            {
                return String.Format("({0} IS NULL)", Feature);
            }
            if (_defaultOnLeft)
            {
                switch (_op)
                {
                    case ComparisonOperator.Greater:
                        return String.Format("(({0} IS NOT NULL) AND ({0} > {1}))", Feature, doubleThreshold);
                    case ComparisonOperator.GreaterOrEqual:
                        return String.Format("(({0} IS NOT NULL) AND ({0} >= {1}))", Feature, doubleThreshold);
                    case ComparisonOperator.Lesser:
                        return String.Format("(({0} IS NULL) OR ({0} < {1}))", Feature, doubleThreshold);
                    case ComparisonOperator.LesserOrEqual:
                        return String.Format("(({0} IS NULL) OR ({0} <= {1}))", Feature, doubleThreshold);
                    default:
                        throw new InvalidOperationException(String.Format("Comparison operator {0} is not valid", _op));
                }
            }
            switch (_op)
            {
                case ComparisonOperator.Greater:
                    return String.Format("(({0} IS NULL) OR ({0} > {1}))", Feature, doubleThreshold);
                case ComparisonOperator.GreaterOrEqual:
                    return String.Format("(({0} IS NULL) OR ({0} >= {1}))", Feature, doubleThreshold);
                case ComparisonOperator.Lesser:
                    return String.Format("(({0} IS NOT NULL) AND ({0} < {1}))", Feature, doubleThreshold);
                case ComparisonOperator.LesserOrEqual:
                    return String.Format("(({0} IS NOT NULL) AND ({0} <= {1}))", Feature, doubleThreshold);
                default:
                    throw new InvalidOperationException(String.Format("Comparison operator {0} is not valid", _op));
            }
        }

        private string ConcatenateList(IList<double> list)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append(String.Format("{0}", list[i]));
                if (i < (list.Count - 1))
                {
                    builder.Append(',');
                }
            }
            return builder.ToString();
        }

        public override IndicatorFunction Clone()
        {
            var indicator = new NumericalIndicator(_feature, _doesDefaultExist, _defaultOnLeft, _integerThreshold, _op);
            indicator.SetFunc();
            return indicator;
        }
    }
}
