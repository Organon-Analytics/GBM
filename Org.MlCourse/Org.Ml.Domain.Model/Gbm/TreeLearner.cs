using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class TreeLearner
    {
        private GbmAlgorithmSettings _algorithmSettings;
        private ModellingDataSettings _dataSettings;
        private DataFrame _frame;

        private const int RootNodeIdx = -1;
        public TreeLearner(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame frame)
        {
            _algorithmSettings = algorithmSettings;
            _dataSettings = dataSettings;
            _frame = frame;
        }

        public GbmTree Train(double[] gradients, double[] hessians)
        {
            _frame.RandomizeTrainingIndices(_algorithmSettings.RowSamplingRate);
            var sampledFeatures = _frame.GetRandomInputList(_algorithmSettings.ColumnSamplingRate);
            var sampledTrainingIndices = _frame.GetRandomTrainingIndices();

            var rootBinInfo = new BinInfo();
            foreach (var idx in sampledTrainingIndices)
            {
                rootBinInfo.Add(1, gradients[idx], hessians[idx]);
            }

            var tree = GbmTree.CreateEmptyTree(RootNodeIdx);
            var splitComputers = new Dictionary<string, SplitComputer>();
            foreach (var feature in sampledFeatures)
            {
                var bin = _frame.GetBin(feature);
                splitComputers.Add(feature, new SplitComputer(feature, bin.NumberOfBins));
            }

            while (true)
            {
                foreach (var feature in sampledFeatures)
                {

                }
            }
            throw new NotImplementedException();
        }
    }
}
