using Org.Infrastructure.Data;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class LeastSquaresLoss : LossFunction
    {
        private float[] _targetArray;
        private float[] _weightArray;
        private int[] _trainingIndices;
        private int[] _validationIndices;
        private int _length;
        private double _initialScore;

        private readonly IBlas _blas;
        private readonly StatsFunctions _funcs;
        //TODO: Creation of DotNetBlas
        public LeastSquaresLoss()
        {
            _targetArray = null;
            _weightArray = null;
            _trainingIndices = null;
            _validationIndices = null;
            _length = -1;
            _initialScore = Double.NaN;

            _blas = new DotNetBlas();
            _funcs = new StatsFunctions(_blas);
        }
        //TODO: StatsFunctions must be handled
        public override void Initialize(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame frame)
        {
            var targetColumnName = dataSettings.TargetColumnName;
            var weightColumnName = dataSettings.WeightColumnName;
            _targetArray = frame.GetDoubleRawArray(targetColumnName);
            if (!String.IsNullOrEmpty(weightColumnName))
            {
                _weightArray = frame.GetDoubleRawArray(weightColumnName);
            }
            _trainingIndices = frame.GetTrainingIndices();
            _validationIndices = frame.GetValidationIndices();
            _length = frame.GetRowCount();

            _initialScore = SetInitialScore();
        }

        public override bool IsGood()
        {
            return true;
        }

        public override void SetInitialScore(double[] scores)
        {
            _blas.Initialize(scores, _initialScore);
        }

        private double SetInitialScore()
        {
            if (_weightArray == null)
            {
                var mean = _funcs.GetMean(_targetArray, _trainingIndices);
                return mean;
            }
            else
            {
                var mean = _funcs.GetMean(_targetArray, _weightArray, _trainingIndices);
                return mean;
            }
        }

        public override double GetInitialScore()
        {
            return _initialScore;
        }

        public override IDictionary<int, double> GetInitialScoreByClass()
        {
            throw new NotImplementedException();
        }

        public override LossFunctionType GetLossFunctionType()
        {
            return LossFunctionType.LeastSquares;
        }

        public override bool IsConstantHessian()
        {
            return (_weightArray == null);
        }

        //TODO: Parallelize and pointerize below
        public override void UpdateGradients(double[] scores, double[] gradients, double[] hessians)
        {
            if (_weightArray == null)
            {
                for (var i = 0; i < _length; ++i)
                {
                    gradients[i] = scores[i] - _targetArray[i];
                    hessians[i] = 1.0;
                }
            }
            else
            {
                for (var i = 0; i < _length; ++i)
                {
                    var w = _weightArray[i];
                    gradients[i] = w * (scores[i] - _targetArray[i]);
                    hessians[i] = w;
                }
            }
        }

        public override void Convert(double score, out double output)
        {
            output = score;
        }

        public override void Convert(double[] scores, double[] output)
        {
            _blas.Copy(scores, output, scores.Length);
        }

        ////TODO: Pointerize nd measure the difference. Should it be _trainingIndices or _rowIndices
        public override double[] LineSearch(double[] scores, double[] delta, double[] grid)
        {
            var gLength = grid.Length;

            var p = 0.0;
            var d = 0.0;
            var s = 0.0;
            var su = 0.0;
            var sum = new double[gLength];

            var t = 0.0;
            foreach (var idx in _trainingIndices)
            {
                s = scores[idx];
                t = _targetArray[idx];
                d = delta[idx];
                for (var j = 0; j < gLength; j++)
                {
                    su = s + grid[j] * d;
                    p = (su - t);
                    sum[j] += p * p;
                }
            }

            var mn = Double.MaxValue;
            var index = -1;
            for (var i = 0; i < sum.Length; i++)
            {
                var val = sum[i];
                if (val >= mn) continue;
                mn = val;
                index = i;
            }
            var res = new double[1];
            res[0] = grid[index];
            return res;
        }

        public override IDictionary<int, double> LineSearch(double[] scores, double[] delta, double[] grid, int[] nodes, int[] nodeIndices)
        {
            var gLength = grid.Length;
            var p = 0.0;
            var d = 0.0;
            var s = 0.0;
            var su = 0.0;
            var n = -1;
            var sum = new Dictionary<int, double[]>();
            foreach (var nodeIdx in nodes)
            {
                sum.Add(nodeIdx, new double[gLength]);
            }

            var t = 0.0;
            foreach (var idx in _trainingIndices)
            {
                s = scores[idx];
                t = _targetArray[idx];
                d = delta[idx];
                n = nodeIndices[idx];
                for (var j = 0; j < gLength; j++)
                {
                    su = s + grid[j] * d;
                    p = (su - t);
                    sum[n][j] += p * p;
                }
            }

            var res = new Dictionary<int, double>();
            var idxMax = -1;
            foreach (var item in sum)
            {
                idxMax = GetIndexOfMax(item.Value);
                res.Add(item.Key, grid[idxMax]);
            }
            return res;
        }

        public override Tuple<double, double> GetLoss(double[] scores)
        {
            var tuple = GetLeastSquaresLoss(scores);
            return new Tuple<double, double>(tuple.Item1.ResidualSumOfSquares, tuple.Item2.ResidualSumOfSquares);
        }

        public override Tuple<LeastSquaresOutput, LeastSquaresOutput> GetLeastSquaresLoss(double[] scores)
        {
            var trainingMetric = _funcs.GetLeastSquaresOutput(scores, _targetArray, _trainingIndices);
            LeastSquaresOutput validationMetric = null;
            if (_validationIndices != null)
            {
                validationMetric = _funcs.GetLeastSquaresOutput(scores, _targetArray, _validationIndices);
            }
            return new Tuple<LeastSquaresOutput, LeastSquaresOutput>(trainingMetric, validationMetric);
        }
    }
}
