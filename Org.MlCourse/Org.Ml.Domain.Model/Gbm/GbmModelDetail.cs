using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    [Serializable]
    public class GbmModelDetail 
    {
        private readonly LossFunctionType _lossFunctionType;
        private readonly Dictionary<string, Bin> _binCollection;

        private readonly SumOfTrees _forest;
        private double _bias;
        private readonly List<LossPerIteration> _lossHistory;

        public GbmModelDetail(LossFunctionType lossFunctionType, Dictionary<string, Bin> binCollection)
        {
            _lossFunctionType = lossFunctionType;
            _binCollection = binCollection;
            _forest = new SumOfTrees();
            _bias = Double.NaN;
            _lossHistory = new List<LossPerIteration>();
        }

        public SumOfTrees SingleForest { get { return _forest; } }
        public double Bias { get { return _bias; } }
        public List<LossPerIteration> LossHistory { get { return _lossHistory; } }

        public void AddLoss(LossPerIteration loss)
        {
            _lossHistory.Add(loss);
        }

        public void SetBias(double bias)
        {
            _bias = bias;
        }

        public void Add(GbmTree tree)
        {
            _forest.Add(tree);
        }

        public string ToSql(IList<string> includedColumns, string scoreColumn, string dataSourceSql)
        {
            var builder = new StringBuilder();
            builder.AppendLine("SELECT");
            if (includedColumns != null && includedColumns.Count > 0)
            {
                foreach (var t in includedColumns)
                {
                    builder.AppendLine(String.Format("{0},", t));
                }
            }
            var transformedScore = GetTransformedScore(scoreColumn);
            builder.AppendLine(String.Format("{0} AS {1}", transformedScore, scoreColumn));
            builder.AppendLine("FROM");
            builder.AppendLine("(");
            builder.AppendLine("SELECT");
            if (includedColumns != null && includedColumns.Count > 0)
            {
                foreach (var t in includedColumns)
                {
                    builder.AppendLine(String.Format("{0},", t));
                }
            }
            builder.AppendLine(String.Format("{0}", _bias));
            if (_forest != null)
            {
                builder.AppendLine("+");
                var formula = _forest.ToSql(_binCollection);
                builder.AppendLine(formula);
                builder.AppendLine(String.Format("AS {0}", scoreColumn));
            }
            builder.AppendLine(dataSourceSql);
            builder.AppendLine(") TOUT");
            return builder.ToString();
        }

        public string GetTransformedScore(string scoreColumn)
        {
            switch (_lossFunctionType)
            {
                case LossFunctionType.LeastSquares:
                    return scoreColumn;
                case LossFunctionType.CrossEntropy:
                    return String.Format("1.0/(1.0 + EXP(-1.0*{0}))", scoreColumn);
                default:
                    throw new NotImplementedException();
            }
        }

        public IList<string> GetUnionOfFormulaVariables()
        {
            return _forest.GetAllFeatures();
        }

        public Tuple<double, double> GetModelPerformance()
        {
            var lastLoss = _lossHistory.Last();
            var trainingLoss = lastLoss.TrainingLoss;
            var validationLoss = lastLoss.ValidationLoss;
            return new Tuple<double, double>(trainingLoss, validationLoss);
        }
    }
}
