using Org.Infrastructure.Data;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    /// <summary>
    /// A LossFunction abstractsand groups the necessary methods to build a Gbm model
    /// It returns the loss given the actual and predicted values among other things
    /// </summary>
    public abstract class LossFunction
    {
        public abstract void Initialize(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame frame);
        /// <summary>
        /// Initializes the scores
        /// </summary>
        /// <param name="scores"></param>
        public abstract void SetInitialScore(double[] scores);
        /// <summary>
        /// Returns the initial scores
        /// </summary>
        /// <returns></returns>
        public abstract double GetInitialScore();
        /// <summary>
        /// Gradients vector is the 1st derivative of the loss with respect to the latest scores
        /// It must be updated after scores are updated
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="gradients"></param>
        /// <param name="hessians"></param>
        public abstract void UpdateGradients(double[] scores, double[] gradients, double[] hessians);
        /// <summary>
        /// Score corresponds to the sum of trees and the prediction is a function of score
        /// Convert method transforms score to prediction
        /// </summary>
        /// <param name="score"></param>
        /// <param name="output"></param>
        public abstract void Convert(double score, out double output);
        public abstract void Convert(double[] scores, double[] output);
        /// <summary>
        /// LineSearch performs a line-search for the update step for the given grid of learning-rate-multipliers
        /// </summary>
        /// <param name="scores"></param>
        /// <param name="delta"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public abstract double[] LineSearch(double[] scores, double[] delta, double[] grid);
        public abstract IDictionary<int, double> LineSearch(double[] scores, double[] delta, double[] grid, int[] nodes, int[] nodeIndices);
        /// <summary>
        /// Get the loss function type
        /// </summary>
        /// <returns></returns>
        public abstract LossFunctionType GetLossFunctionType();
        /// <summary>
        /// Get the loss value given the scores (loss is the distance between actual and predictions)
        /// </summary>
        /// <param name="scores"></param>
        /// <returns></returns>
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
