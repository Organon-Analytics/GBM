using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model
{
    public class SplitComputer
    {
        private readonly string _featureName;
        private readonly int _numBins;
        private readonly IDictionary<int, BinInfo[]> _data;

        public SplitComputer(string featureName, int numBins)
        {
            _featureName = featureName;
            _numBins = numBins;
            _data = new Dictionary<int, BinInfo[]>();
        }


        public void Add(IList<int> nodes)
        {
            foreach (var nodeId in nodes)
            {
                _data.Add(nodeId, new BinInfo[_numBins]);
            }
        }

        public void Remove(IList<int> nodes)
        {
            foreach (var nodeId in nodes)
            {
                _data.Remove(nodeId);
            }
        }

        public void Complete(int parentNode, int smallNode, int largeNode)
        {
            _data.Add(largeNode, new BinInfo[_numBins]);
            var parentData = _data[parentNode];
            var smallData = _data[smallNode];
            var largeData = _data[largeNode];
            for (var i = 0; i < _numBins; i++)
            {
                var parentBin = parentData[i];
                var smallBin = smallData[i];
                largeData[i].Overwrite(parentBin.SumWeights - smallBin.SumWeights,
                    parentBin.SumGradients - smallBin.SumGradients,
                    parentBin.SumHessians - smallBin.SumHessians);
            }
        }

        public void Aggregate(int length, int[] rowIndices, int[] nodeIndices, int[] data, double[] weights, double[] gradients, double[] hessians)
        {
            var g = -1.0;
            var p = 1.0;
            var rowId = -1;
            var nodeId = -1;
            var binId = -1;
            for (var i = 0; i < length; i++)
            {
                rowId = rowIndices[i];
                binId = data[rowId];
                nodeId = nodeIndices[i];
                _data[nodeId][binId].Add(weights[i], gradients[i], hessians[i]);
            }
        }

        //private void FindBestCategoricalSplit(BinInfo parent, BinInfo[] data, SplitInfo split,
        //                                      bool doesDefaultExist, Gbm.GbmAlgorithmSettings algorithmSettings)
        //{

        //}
    }
}
