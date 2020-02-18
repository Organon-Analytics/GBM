using Org.Infrastructure.Data;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class CrossEntropyLoss : LossFunction
    {
        private int[] _target;
        private float[] _weights;
        private int[] _trainingIndices;
        private int[] _validationIndices;
        private int _length;
        private int _pLabel;
        private int _nLabel;
        private IDictionary<int, double> _labelValues;
        private IDictionary<int, double> _labelWeights;
        private readonly IBlas _blas;
        private readonly StatsFunctions _funcs;
        private double _initialScore;
        private bool _isGood;
        public CrossEntropyLoss()
        {
            _target = null;
            _weights = null;
            _trainingIndices = null;
            _validationIndices = null;
            _length = -1;
            _pLabel = -1;
            _nLabel = -1;
            _labelValues = null;
            _labelWeights = null;
            _initialScore = Double.NaN;
            _isGood = false;

            _blas = new DotNetBlas();
            _funcs = new StatsFunctions(_blas);
        }

        public override void Initialize(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame frame)
        {
            var targetColumnName = dataSettings.TargetColumnName;
            var weightColumnName = dataSettings.WeightColumnName;
            _target = frame.GetIntegerArray(targetColumnName);
            if (!String.IsNullOrEmpty(weightColumnName))
            {
                _weights = frame.GetDoubleRawArray(weightColumnName);
            }
            _trainingIndices = frame.GetTrainingIndices();
            _validationIndices = frame.GetValidationIndices();
            _length = frame.GetRowCount();

            #region Looks deprecated
            var targetBin = frame.GetBin(targetColumnName);
            var pRawLabel = dataSettings.PositiveCategory;
            var nRawLabel = dataSettings.NegativeCategory;
            if (targetBin.BinType == BinType.Categorical)
            {
                _pLabel = targetBin.GetIndex(pRawLabel);
                _nLabel = targetBin.GetIndex(nRawLabel);
            }
            else
            {
                var pLabelAsDouble = Double.NaN;
                var nLabelAsDouble = Double.NaN;
                var pSuccess = Double.TryParse(pRawLabel, out pLabelAsDouble);
                var nSuccess = Double.TryParse(nRawLabel, out nLabelAsDouble);
                if (pSuccess && nSuccess)
                {
                    _pLabel = targetBin.GetIndex(pLabelAsDouble);
                    _nLabel = targetBin.GetIndex(nLabelAsDouble);
                    _isGood = true;
                }
                else
                {
                    _isGood = false;
                }
            }
            #endregion

            _labelValues = new Dictionary<int, double> { { _pLabel, 1.0 }, { _nLabel, 0.0 } };
            _labelWeights = new Dictionary<int, double>
            {
                {_pLabel, 1.0},
                {_nLabel, 1.0}
            };
            _initialScore = SetInitialScore();
        }

        public override void SetInitialScore(double[] scores)
        {
            _blas.Initialize(scores, _initialScore);
        }

        //TODO: Handle extreme cases below (p in (0,1))
        private double SetInitialScore()
        {
            if (_weights == null)
            {
                var num = 0.0;
                var denom = 0.0;
                foreach (var idx in _trainingIndices)
                {
                    var val = _target[idx];
                    var w = _labelWeights[val];
                    num += w * _labelValues[val];
                    denom += w;
                }
                var p = (num / denom);
                return Math.Log(p / (1.0 - p));
            }
            else
            {
                var num = 0.0;
                var denom = 0.0;
                foreach (var idx in _trainingIndices)
                {
                    var val = _target[idx];
                    var w = _labelWeights[val] * _weights[val];
                    num += w * _labelValues[val];
                    denom += w;
                }
                var p = (num / denom);
                return Math.Log(p / (1.0 - p));
            }
        }

        public override double GetInitialScore()
        {
            return _initialScore;
        }

        public override LossFunctionType GetLossFunctionType()
        {
            return LossFunctionType.CrossEntropy;
        }

        public override void UpdateGradients(double[] score, double[] gradients, double[] hessians)
        {
            var p = Double.NaN;
            var y = -1;
            var w = Double.NaN;
            if (_weights == null)
            {
                for (var i = 0; i < _length; ++i)
                {
                    p = 1.0 / (1.0 + Math.Exp(-1.0 * score[i]));
                    y = _target[i];
                    w = _labelWeights[y];
                    gradients[i] = (p - _labelValues[y]) * w;
                    hessians[i] = p * (1.0 - p) * w;
                }
            }
            else
            {
                for (var i = 0; i < _length; ++i)
                {
                    p = 1.0 / (1.0 + Math.Exp(-1.0 * score[i]));
                    y = _target[i];
                    w = _labelWeights[y] * _weights[i];
                    gradients[i] = (p - _labelValues[y]) * w;
                    hessians[i] = p * (1.0 - p) * w;
                }
            }
        }
        public override void Convert(double score, out double output)
        {
            output = 1.0 / (1.0 + Math.Exp(-1.0 * score));
        }
        public override void Convert(double[] scores, double[] output)
        {
            for (var i = 0; i < scores.Length; i++)
            {
                output[i] = 1.0 / (1.0 + Math.Exp(-1.0 * scores[i]));
            }
        }
        //TODO: Pointerize and measure the difference. Should it be _trainingIndices or _rowIndices
        public override double[] LineSearch(double[] scores, double[] delta, double[] grid)
        {
            var gLength = grid.Length;
            var t = 0.0;
            var p = 0.0;
            var d = 0.0;
            var b = false;
            var s = 0.0;
            var sum = new double[gLength];
            foreach (var idx in _validationIndices)
            {
                t = scores[idx];
                d = delta[idx];
                b = (_target[idx] == _pLabel);
                for (var j = 0; j < gLength; j++)
                {
                    s = t + grid[j] * d;
                    p = 1.0 / (1.0 + Math.Exp(-1.0 * s));
                    sum[j] += b ? Math.Log(p) : Math.Log(1.0 - p);
                }
            }
            var mx = Double.MinValue;
            var index = -1;
            for (var i = 0; i < sum.Length; i++)
            {
                var val = sum[i];
                if (val <= mx) continue;
                mx = val;
                index = i;
            }
            var res = new double[1];
            res[0] = grid[index];
            Console.WriteLine("Winning learning rate: {0}", grid[index]);
            return res;
        }

        public override IDictionary<int, double> LineSearch(double[] scores, double[] delta, double[] grid, int[] nodes, int[] nodeIndices)
        {
            var gLength = grid.Length;
            var t = 0.0;
            var p = 0.0;
            var d = 0.0;
            var b = false;
            var s = 0.0;
            var n = -1;
            var sum = new Dictionary<int, double[]>();
            foreach (var nodeIdx in nodes)
            {
                sum.Add(nodeIdx, new double[gLength]);
            }
            foreach (var idx in _validationIndices)
            {
                t = scores[idx];
                d = delta[idx];
                b = (_target[idx] == _pLabel);
                n = nodeIndices[idx];
                for (var j = 0; j < gLength; j++)
                {
                    s = t + grid[j] * d;
                    p = 1.0 / (1.0 + Math.Exp(-1.0 * s));
                    sum[n][j] += b ? Math.Log(p) : Math.Log(1.0 - p);
                }
            }
            var res = new Dictionary<int, double>();
            foreach (var item in sum)
            {
                var maxIndex = GetIndexOfMax(item.Value);
                Console.WriteLine("Node: {0}, Best-rate: {1}", item.Key, grid[maxIndex]);
                res.Add(item.Key, grid[maxIndex]);
            }
            return res;
        }

        public override Tuple<double, double> GetLoss(double[] scores)
        {
            var loss = GetLoglikelihoodLoss(scores);
            return loss;
        }

        public override Tuple<double, double> GetLoglikelihoodLoss(double[] scores)
        {
            var trainingMetric = GetMinusLogLikelihood(_trainingIndices, scores);
            if ((_validationIndices == null) || (_validationIndices.Length == 0))
                return new Tuple<double, double>(trainingMetric, Double.NaN);
            var validationMetric = GetMinusLogLikelihood(_validationIndices, scores);
            return new Tuple<double, double>(trainingMetric, validationMetric);
        }

        private double GetMinusLogLikelihood(IList<int> indices, double[] scores)
        {
            double p;
            var logL = 0.0;
            foreach (var idx in indices)
            {
                p = 1.0 / (1.0 + Math.Exp(-1.0 * scores[idx]));
                logL += (_target[idx] == _pLabel) ? Math.Log(p) : Math.Log(1.0 - p);
            }
            return -1.0 * logL;
        }

        //TODO: Optimize below
        public override Tuple<AreaUnderTheCurve, AreaUnderTheCurve> GetAreaUnderTheCurve(double[] scores)
        {
            var trainingAuc = _funcs.ComputeAuc(scores, _target, _pLabel, _nLabel, _trainingIndices);
            AreaUnderTheCurve validationAuc = null;
            if (_validationIndices != null)
            {
                validationAuc = _funcs.ComputeAuc(scores, _target, _pLabel, _nLabel, _validationIndices);
            }
            var result = new Tuple<AreaUnderTheCurve, AreaUnderTheCurve>(trainingAuc, validationAuc);
            return result;
        }
    }
}
