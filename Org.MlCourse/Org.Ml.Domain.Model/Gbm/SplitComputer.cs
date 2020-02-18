using Org.Infrastructure.Data;
using Org.Ml.Domain.Model.Gbm;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model
{
    public class SplitComputer
    {
        private readonly string _featureName; //INCOME
        private BinType _binType;
        private readonly int _numBins;
        private readonly IDictionary<int, BinInfo[]> _data; // 0, 1

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

        public void FindBestSplit(IDictionary<int, SplitInfo> splits, GbmTreeNodePool pool, GbmAlgorithmSettings algorithmSettings)
        {
            var regL1 = algorithmSettings.RegL1;
            var regL2 = algorithmSettings.RegL2;
            var nodes = splits.Keys;
            foreach (var nodeId in nodes)
            {
                var parentT = pool.GetBinInfoTraining(nodeId);
                var parentTGain = GetLeafSplitGain(parentT, regL1, regL2);
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

        private void ScanFromLeft(BinInfo parent, double parentTGain, BinInfo[] bins, SplitInfo split, bool doesDefaultExist, GbmAlgorithmSettings algorithmSettings)
        {
            var minWeightPerLeaf = algorithmSettings.MinWeightPerLeaf;
            var minHessianPerLeaf = algorithmSettings.MinHessianPerLeaf;
            var regL1 = algorithmSettings.RegL1;
            var regL2 = algorithmSettings.RegL2;

            var defaultBinInfo = bins[0];
            var tLeft = new BinInfo(defaultBinInfo);
            var tRight = new BinInfo(parent);
            tRight.Subtract(defaultBinInfo);

            var tLeftBest = new BinInfo();
            var tRightBest = new BinInfo();

            var bestGain = Double.NegativeInfinity;
            var bestThreshold = -1;
            var isSplittable = false;
            for (var i = 1; i < bins.Length; ++i)
            {
                var tBin = bins[i];
                tLeft.Add(tBin);
                tRight.Subtract(tBin);

                if (tLeft.SumWeights < minWeightPerLeaf) continue;
                if (tLeft.SumHessians < minHessianPerLeaf) continue;
                if (tRight.SumWeights < minWeightPerLeaf) break;
                if (tRight.SumHessians < minHessianPerLeaf) break;

                var leftGainT = GetLeafSplitGain(tLeft, regL1, regL2);
                var rightGainT = GetLeafSplitGain(tRight, regL1, regL2);
                var currentGainT = leftGainT + rightGainT;
                if (currentGainT <= parentTGain) continue;
                isSplittable = true;
                if (currentGainT > bestGain)
                {
                    tLeftBest.Overwrite(tLeft);
                    tRightBest.Overwrite(tRight);
                    bestThreshold = i;
                    bestGain = currentGainT;
                }
            }
            if (isSplittable && (bestGain > split.Gain))
            {
                split.Feature = _featureName;
                split.IsNumerical = true;
                split.IsNull = false;
                split.IntegerThreshold = bestThreshold;
                split.LeftPrediction = CalculateSplittedLeafOutput(tLeftBest, regL1, regL2);
                split.LeftInfoTraining = new BinInfo(tLeftBest);
                split.RightPrediction = CalculateSplittedLeafOutput(tRightBest, regL1, regL2);
                split.RightInfoTraining = new BinInfo(tRightBest);
                split.Gain = bestGain;
                split.DefaultOnLeft = true;
                split.DoesDefaultExist = doesDefaultExist;
                split.OrphanPrediction = CalculateSplittedLeafOutput(parent, regL1, regL2);
            }
        }

        private void ScanFromRight(BinInfo parent, double parentInformation, BinInfo[] bins, SplitInfo split, bool doesDefaultExist, GbmAlgorithmSettings algorithmSettings)
        {
            var minWeightPerLeaf = algorithmSettings.MinWeightPerLeaf;
            var minHessianPerLeaf = algorithmSettings.MinHessianPerLeaf;
            var regL1 = algorithmSettings.RegL1;
            var regL2 = algorithmSettings.RegL2;

            var defaultBinInfo = bins[0];
            var tLeft = new BinInfo(parent);
            tLeft.Subtract(defaultBinInfo);
            var tRight = new BinInfo(defaultBinInfo);

            var tLeftBest = new BinInfo();
            var tRightBest = new BinInfo();

            var bestGain = Double.NegativeInfinity;
            var bestThreshold = -1;
            var isSplittable = false;
            for (var i = (bins.Length - 1); i >= 1; --i)
            {
                var tBin = bins[i];
                tRight.Add(tBin);
                tLeft.Subtract(tBin);

                if (tRight.SumWeights < minWeightPerLeaf) continue;
                if (tRight.SumHessians < minHessianPerLeaf) continue;
                if (tLeft.SumWeights < minWeightPerLeaf) break;
                if (tLeft.SumHessians < minHessianPerLeaf) break;

                var leftGain = GetLeafSplitGain(tLeft, regL1, regL2);
                var rightGain = GetLeafSplitGain(tRight, regL1, regL2);
                var currentGain = leftGain + rightGain;
                // gain with split is worse than without split
                if (currentGain <= parentInformation) continue;
                isSplittable = true;
                if (currentGain > bestGain)
                {
                    tLeftBest.Overwrite(tLeft);
                    tRightBest.Overwrite(tRight);
                    bestThreshold = i - 1;
                    bestGain = currentGain;
                }
            }
            //Console.WriteLine("Numerical column: {0}, Right Scan Best Gain: {1}", _featureName, bestGain);
            if (isSplittable && (bestGain > split.Gain))
            {
                // Update split information
                split.Feature = _featureName;
                split.IsNumerical = true;
                split.IsNull = false;
                split.IntegerThreshold = bestThreshold;
                split.LeftPrediction = CalculateSplittedLeafOutput(tLeftBest, regL1, regL2);
                split.LeftInfoTraining = new BinInfo(tLeftBest);
                split.RightPrediction = CalculateSplittedLeafOutput(tRightBest, regL1, regL2);
                split.RightInfoTraining = new BinInfo(tRightBest);
                split.Gain = bestGain;
                split.DefaultOnLeft = false;
                split.DoesDefaultExist = doesDefaultExist;
                split.OrphanPrediction = CalculateSplittedLeafOutput(parent, regL1, regL2);
            }
        }

        private void FindBestCategoricalSplit(BinInfo parent, double parentInformation, BinInfo[] bins, SplitInfo split,
            bool doesDefaultExist, GbmAlgorithmSettings algorithmSettings)
        {
            var list = new List<Tuple<double, int>>();
            for (var i = 0; i < bins.Length; i++)
            {
                var bin = bins[i];
                if (bin.SumWeights < algorithmSettings.CategoricalSmoothing) continue;
                var sumGradients = bin.SumGradients;
                var sumHessians = bin.SumHessians;
                var denominator = sumHessians + algorithmSettings.CategoricalSmoothing;
                list.Add(new Tuple<double, int>(sumGradients / denominator, i));
            }
            list.Sort();
            if (list.Count <= 1) return;

            var directions = new List<KeyValuePair<int, int>>
            {
                new KeyValuePair<int, int>(0, 1),
                new KeyValuePair<int, int>(list.Count - 1, -1)
            };
            var maxCat = Math.Min(algorithmSettings.MaxCategoryProcessed, (list.Count + 1) / 2);

            var minWeightPerLeaf = algorithmSettings.MinWeightPerLeaf;
            var minHessianPerLeaf = algorithmSettings.MinHessianPerLeaf;
            var regL1 = algorithmSettings.RegL1;
            var regL2 = algorithmSettings.RegL2;
            foreach (var pair in directions)
            {
                var tLeft = new BinInfo();
                var tRight = new BinInfo(parent);

                var tLeftBest = new BinInfo();
                var tRightBest = new BinInfo();

                var bestGain = Double.NegativeInfinity;
                var bestThreshold = -1;
                var isSplittable = false;

                var offset = pair.Key;
                var step = pair.Value;
                var mark = offset;
                for (var i = 0; i < maxCat; i++, mark += step)
                {
                    var tuple = list[mark];

                    var binId = tuple.Item2;
                    var binInfo = bins[binId];
                    tLeft.Add(binInfo);
                    tRight.Subtract(binInfo);


                    if (tLeft.SumWeights < minWeightPerLeaf) continue;
                    if (tLeft.SumHessians < minHessianPerLeaf) continue;
                    if (tRight.SumWeights < minWeightPerLeaf) break;
                    if (tRight.SumHessians < minHessianPerLeaf) break;

                    var leftGain = GetLeafSplitGain(tLeft, regL1, regL2);
                    var rightGain = GetLeafSplitGain(tRight, regL1, regL2);
                    var currentGain = leftGain + rightGain;
                    if (currentGain <= parentInformation) continue;
                    isSplittable = true;
                    if (currentGain > bestGain)
                    {
                        tLeftBest.Overwrite(tLeft);
                        tRightBest.Overwrite(tRight);
                        bestThreshold = mark;
                        bestGain = currentGain;
                    }
                }
                if (isSplittable && (bestGain > split.Gain))
                {
                    split.Feature = _featureName;
                    split.IsNumerical = false;
                    split.IsNull = false;
                    split.LeftCategoryIndices = GetLeftCategoryIndices(list, bestThreshold, offset);
                    split.DefaultOnLeft = split.LeftCategoryIndices.Contains(0);
                    split.LeftPrediction = CalculateSplittedLeafOutput(tLeftBest, regL1, regL2);
                    split.LeftInfoTraining = new BinInfo(tLeftBest);
                    split.RightPrediction = CalculateSplittedLeafOutput(tRightBest, regL1, regL2);
                    split.RightInfoTraining = new BinInfo(tRightBest);
                    split.Gain = bestGain;
                    split.DoesDefaultExist = doesDefaultExist;
                    split.OrphanPrediction = CalculateSplittedLeafOutput(parent, regL1, regL2);
                }
            }
        }

        private IList<int> GetLeftCategoryIndices(IList<Tuple<double, int>> sortedTuples, int bestThreshold, int offset)
        {
            var indices = new List<int>();
            var minIdx = Math.Min(bestThreshold, offset);
            var maxIdx = Math.Max(bestThreshold, offset);
            for (var i = minIdx; i <= maxIdx; i++)
            {
                var tuple = sortedTuples[i];
                var idx = tuple.Item2;
                indices.Add(idx);
            }
            return indices;
        }
        private double GetLeafSplitGain(BinInfo info, double l1, double l2)
        {
            var sumGradients = info.SumGradients;
            var sumHessians = info.SumHessians;
            var absSumGradients = Math.Abs(sumGradients);
            var regAbsSumGradients = Math.Max(0.0, absSumGradients - l1);
            return (regAbsSumGradients * regAbsSumGradients) / (sumHessians + l2);
        }

        private double CalculateSplittedLeafOutput(BinInfo info, double l1, double l2)
        {
            var sumGradients = info.SumGradients;
            var sumHessians = info.SumHessians;
            var absSumGradients = Math.Abs(sumGradients);
            var regAbsSumGradients = Math.Max(0.0, absSumGradients - l1);
            return (-1.0 * regAbsSumGradients * Math.Sign(sumGradients)) / (sumHessians + l2);
        }
    }
}
