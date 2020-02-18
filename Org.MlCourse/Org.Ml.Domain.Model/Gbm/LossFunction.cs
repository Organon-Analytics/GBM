﻿using Org.Infrastructure.Data;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    /// <summary>
    /// A LossFunction abstractsand groups the necessary methods to build a Gbm model
    /// Specifically, it returns the loss given the actual and predicted values
    /// </summary>
    public abstract class LossFunction
    {
        public abstract void Initialize(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame frame);
        public abstract bool IsGood();
        public abstract void SetInitialScore(double[] scores);
        public abstract double GetInitialScore();
        public abstract IDictionary<int, double> GetInitialScoreByClass();
        public abstract void UpdateGradients(double[] scores, double[] gradients, double[] hessians);
        public abstract void Convert(double score, out double output);
        public abstract void Convert(double[] scores, double[] output);
        public abstract double[] LineSearch(double[] scores, double[] delta, double[] grid);
        public abstract IDictionary<int, double> LineSearch(double[] scores, double[] delta, double[] grid, int[] nodes, int[] nodeIndices);
        public abstract LossFunctionType GetLossFunctionType();
        public abstract bool IsConstantHessian();
        public abstract Tuple<double, double> GetLoss(double[] scores);

        public virtual Tuple<LeastSquaresOutput, LeastSquaresOutput> GetLeastSquaresLoss(double[] scores)
        {
            throw new InvalidOperationException("L2-Loss is not valid for this problem");
        }

        public virtual Tuple<double, double> GetLoglikelihoodLoss(double[] scores)
        {
            throw new InvalidOperationException("Loglikelihood-Loss is not valid for this problem");
        }

        public virtual Tuple<AreaUnderTheCurve, AreaUnderTheCurve> GetAreaUnderTheCurve(double[] scores)
        {
            throw new InvalidOperationException("Auc information is valid only for Binary Classification problems");
        }

        public static LossFunction CreateLossFunction(LossFunctionType type)
        {
            switch (type)
            {
                case LossFunctionType.LeastSquares:
                    return new LeastSquaresLoss();
                case LossFunctionType.CrossEntropy:
                    return new CrossEntropyLoss();
                default:
                    throw new NotImplementedException();
            }
        }

        protected int GetIndexOfMax(double[] array)
        {
            if (array == null || array.Length == 0) return -1;
            var idx = 0;
            var mx = array[0];
            for (var i = 1; i < array.Length; i++)
            {
                var val = array[i];
                if (val <= mx) continue;
                mx = val;
                idx = i;
            }
            return idx;
        }
    }
}
