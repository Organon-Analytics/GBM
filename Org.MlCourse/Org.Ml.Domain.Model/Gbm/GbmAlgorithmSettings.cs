using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class GbmAlgorithmSettings
    {
        public GbmAlgorithmSettings()
        {
            LearningRate = 0.01;          
            MaxDepth = 5;
            MaxLeaves = 2;
            NumIterations = 1000;
            RowSamplingRate = 1.0;
            ColumnSamplingRate = 1.0;
            MinWeightPerLeaf = 20.0;
            MinHessianPerLeaf = 1.0;
            MaxBins = 512;
            MinSplitGain = 0.0;
            RegL1 = 0.0;
            RegL2 = 1.0;           
            CategoricalSmoothing = 10.0;
            MinCategorySize = 50.0;
            MaxCategoryProcessed = 16;

            LossFunctionType = LossFunctionType.LeastSquares;
            
            SearchForHyperParameters = false;
            UseLineSearchForLearningRate = false;
        }
        public double LearningRate { get; set; }
        public int MaxDepth { get; set; }
        public int MaxLeaves { get; set; }
        public int NumIterations { get; set; }
        public double RowSamplingRate { get; set; }
        public double ColumnSamplingRate { get; set; }
        public int MaxBins { get; set; }
        public double MinSplitGain { get; set; }
        public double MinWeightPerLeaf { get; set; }
        public double MinHessianPerLeaf { get; set; }
        public double RegL1 { get; set; }
        public double RegL2 { get; set; }
        public double CategoricalSmoothing { get; set; }
        public double MinCategorySize { get; set; }
        public int MaxCategoryProcessed { get; set; }

        public LossFunctionType LossFunctionType { get; set; }

        public bool SearchForHyperParameters { get; set; }
        public bool UseLineSearchForLearningRate { get; set; }
    }
}
