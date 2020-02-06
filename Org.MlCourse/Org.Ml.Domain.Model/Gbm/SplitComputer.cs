using Org.Infrastructure.Data;
using Org.Ml.Domain.Model.Gbm;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model
{
    public class SplitComputer
    {
        private readonly string _featureName;
        private BinType _binType;
        private readonly int _numBins;
        private readonly IDictionary<int, BinInfo[]> _data;

        public SplitComputer(string featureName, int numBins, BinType binType)
        {
            _featureName = featureName;
            _binType = binType;
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

        public void FindBestSplit(IDictionary<int, SplitInfo> splits, TreeNodePool pool, GbmAlgorithmSettings algorithmSettings)
        {
            var nodes = splits.Keys;
            foreach (var nodeId in nodes)
            {
                var parentT = pool.GetBinInfoTraining(nodeId);
                var parentTGain = GetLeafSplitGain(parentT);
                var binsT = _data[nodeId];
                

                var split = splits[nodeId];
                var defaultBinInfo = binsT[0];
                var doesDefaultExist = (defaultBinInfo.SumWeights > 0);
                split.DefaultIdx = Bin.DefaultIndex;
                if (_binType == BinType.Numerical)
                {
                    ScanFromLeft(parentT, parentTGain, binsT, split, doesDefaultExist, algorithmSettings);
                    if (doesDefaultExist)
                    {
                        ScanFromRight(parentT, parentTGain, binsT, split, true, algorithmSettings);
                    }
                }
                else if (_binType == BinType.Categorical)
                {
                    FindBestCategoricalSplit(parentT, parentTGain, binsT, split, doesDefaultExist, algorithmSettings);
                }
                else
                {
                    throw new InvalidOperationException("Invalid BinType enumeration");
                }
            }
        }

        private void ScanFromRight(BinInfo parentT, double parentTGain, BinInfo[] binsT, SplitInfo split, bool v, GbmAlgorithmSettings algorithmSettings)
        {
            throw new NotImplementedException();
        }

        private void ScanFromLeft(BinInfo parentT, double parentTGain, BinInfo[] binsT, SplitInfo split, bool doesDefaultExist, GbmAlgorithmSettings algorithmSettings)
        {
            throw new NotImplementedException();
        }

        private double GetLeafSplitGain(BinInfo info)
        {
            var sumGradients = info.SumGradients;
            var sumHessians = info.SumHessians;
            var absSumGradients = Math.Abs(sumGradients);
            return (absSumGradients * absSumGradients) / (sumHessians);
        }


        private void FindBestCategoricalSplit(BinInfo parent, double parentInformation, BinInfo[] bins, SplitInfo split, 
            bool doesDefaultExist, GbmAlgorithmSettings algorithmSettings)
        {
            throw new NotImplementedException();
        }
    }
}
