using Org.Infrastructure.Data;
using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class GbmTreeLearner
    {
        private double[] _weights;
        private GbmAlgorithmSettings _algorithmSettings;
        private ModellingDataSettings _dataSettings;
        private DataFrame _frame;

        private int[] _nodeIndices;
        private string[] _eligibleInputColumns;
        private IBlas _blas;

        private const int RootNodeIdx = -1;
        public GbmTreeLearner(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame frame)
        {
            _algorithmSettings = algorithmSettings;
            _dataSettings = dataSettings;
            _frame = frame;
            _blas = new DotNetBlas();
        }

        public void Initialize()
        {
            var rowCount = _frame.GetRowCount();
            _nodeIndices = new int[rowCount];
            _blas.Initialize(_nodeIndices, RootNodeIdx);
            _eligibleInputColumns = _frame.GetEligibleInputColumns(_dataSettings.IncludedColumns, new List<string> { _dataSettings.TargetColumnName });
            SetWeightArray();
        }

        private void SetWeightArray()
        {
            _weights = new double[_frame.GetRowCount()];
            _blas.Initialize(_weights, 1.0);
        }

        private void ResetNodeIndices(int id)
        {
            _blas.Initialize(_nodeIndices, id);
        }

        public GbmTree Train(double[] gradients, double[] hessians)
        {
            //Randomize rows
            _frame.RandomizeTrainingIndices(_algorithmSettings.RowSamplingRate);
            //Randomize features
            var sampledFeatures = _frame.GetRandomInputList(_algorithmSettings.ColumnSamplingRate);

            var sampledTrainingIndices = _frame.GetRandomTrainingIndices();
            var validationIndices = _frame.GetValidationIndices();

            var rootBinInfoTraining = new BinInfo();
            foreach (var idx in sampledTrainingIndices)
            {
                rootBinInfoTraining.Add(1, gradients[idx], hessians[idx]);
            }

            var rootBinInfoValidation = new BinInfo();
            foreach (var idx in validationIndices)
            {
                rootBinInfoValidation.Add(1, gradients[idx], hessians[idx]);
            }

            var nodePool = new GbmTreeNodePool(RootNodeIdx, rootBinInfoTraining, rootBinInfoValidation);
            ResetNodeIndices(RootNodeIdx);

            var tree = GbmTree.CreateEmptyTree(RootNodeIdx);
            var splitComputers = new Dictionary<string, SplitComputer>();
            foreach (var feature in sampledFeatures)
            {
                var bin = _frame.GetBin(feature);
                splitComputers.Add(feature, new SplitComputer(feature, bin.NumberOfBins, bin.BinType));
            }

            var sampledTrainingLength = sampledTrainingIndices.Length;
            var orderedRowIndicesT = new int[sampledTrainingLength];
            var orderedNodeIndicesT = new int[sampledTrainingLength];
            var orderedWeightsT = new double[sampledTrainingLength];
            var orderedGradientsT = new double[sampledTrainingLength];
            var orderedHessiansT = new double[sampledTrainingLength];

            var smallNodes = new List<int> { RootNodeIdx };
            var nodeRelations = nodePool.NodeRelations;
            //Produces the tree
            while (true)
            {
                var nodeStatusBeforeSplit = nodePool.DeepCopyLifeStatus();
                var splits = new Dictionary<int, SplitInfo>();
                foreach (var item in nodeStatusBeforeSplit)
                {
                    if (item.Value)
                    {
                        splits.Add(item.Key, new SplitInfo());
                    }
                }

                var lengthTraining = UpdateOrderedData(smallNodes, sampledTrainingIndices, orderedRowIndicesT, _nodeIndices,
                    orderedNodeIndicesT, _weights, orderedWeightsT, gradients, orderedGradientsT, hessians,
                    orderedHessiansT, 0);

                foreach (var feature in sampledFeatures)
                {
                    var data = _frame.GetIntegerArray(feature);
                    var splitComputer = splitComputers[feature];
                    splitComputer.Add(smallNodes);
                    splitComputer.Aggregate(lengthTraining, orderedRowIndicesT, orderedNodeIndicesT, data, orderedWeightsT, orderedGradientsT, orderedHessiansT);
                    foreach (var item in nodeRelations)
                    {
                        var smallNode = item.Key;
                        var largeNode = item.Value.Key;
                        var parentNode = item.Value.Value;
                        splitComputer.Complete(parentNode, smallNode, largeNode);
                    }
                    splitComputer.FindBestSplit(splits, nodePool, _algorithmSettings);
                }

                var selectedNodes = GetNodesToBeSplit(splits, nodePool, _algorithmSettings.MaxLeaves);
                var isSuccess = (selectedNodes.Count > 0);

                if (!isSuccess) break;

                UpdateTreeAndNodePool(selectedNodes, splits, nodePool, tree);

                smallNodes = nodeRelations.Keys.ToList();
                smallNodes.Sort();

                UpdateNodeIndices(selectedNodes, splits, tree);
                var numLeafNodes = tree.GetNumLeafNodes();
                if (numLeafNodes >= _algorithmSettings.MaxLeaves) break;
            }
            throw new NotImplementedException();
        }

        private int UpdateOrderedData(List<int> smallNodes, int[] indices, int[] oIndices, int[] nodeIndices,
            int[] oNodeIndices, double[] weights, double[] oWeights, double[] gradients, double[] oGradients,
            double[] hessians, double[] oHessians, int offset)
        {
            var nodeId = -1;
            var found = false;
            var cursor = 0;
            var mark = -1;
            for (var i = 0; i < indices.Length; i++)
            {
                var idx = indices[i];
                nodeId = nodeIndices[idx];
                found = smallNodes.BinarySearch(nodeId) >= 0;
                if (!found) continue;
                oIndices[cursor] = idx;
                oNodeIndices[cursor] = nodeId;
                oWeights[cursor] = weights[idx];
                mark = idx + offset;
                oGradients[cursor] = gradients[mark];
                oHessians[cursor] = hessians[mark];
                ++cursor;
            }
            return cursor;
        }

        private IList<int> GetNodesToBeSplit(IDictionary<int, SplitInfo> splits, GbmTreeNodePool pool, int capacity)
        {
            var allNodes = pool.NodeLifeStatus.Keys.ToList();
            var list = new List<Tuple<double, int>>();
            foreach (var nodeId in allNodes)
            {
                var isAlive = pool.NodeLifeStatus[nodeId];
                if (!isAlive) continue;
                var split = splits[nodeId];
                if (split.IsNull) continue;
                list.Add(new Tuple<double, int>(split.Gain, nodeId));
            }
            if (list.Count == 0) return new List<int>();
            list.Sort();
            var end = list.Count - 1;
            var beg = Math.Max(0, (list.Count - capacity / 2));

            //Console.WriteLine("List:{0}, Capacity: {1}", list.Count, capacity);
            var selectedNodes = new List<int>();
            for (var i = end; i >= beg; i--)
            {
                var tuple = list[i];
                selectedNodes.Add(tuple.Item2);
            }
            return selectedNodes;
        }

        private void UpdateTreeAndNodePool(IList<int> selectedNodes, IDictionary<int, SplitInfo> splits, GbmTreeNodePool pool, GbmTree tree)
        {
            pool.ClearNodeRelations();
            foreach (var nodeId in selectedNodes)
            {
                var split = splits[nodeId];
                var doesDefaultExist = split.DoesDefaultExist;
                var parentBinInfoT = pool.GetBinInfoTraining(nodeId);
                var parentBinInfoV = pool.GetBinInfoValidation(nodeId);
                var leftBinInfoT = split.GetLeftBinInfoTraining();
                var rightBinInfoT = split.GetRightBinInfoTraining();
                var leftBinInfoV = split.GetLeftBinInfoValidation();
                var rightBinInfoV = split.GetRightBinInfoValidation();

                var currentTreeNode = tree.GetNode(nodeId);
                currentTreeNode.SplitFeature = split.Feature;
                var orphanBinInfoT = parentBinInfoT.DeepCopy();
                var orphanBinInfoV = parentBinInfoV.DeepCopy();
                var leftIndicator = split.GetLeftIndicator();
                var leftPrediction = split.LeftPrediction;
                var rightIndicator = split.GetRightIndicator();
                var rightPrediction = split.RightPrediction;
                if (doesDefaultExist)
                {
                    if (split.IsNumerical)
                    {
                        var pair = pool.GenerateNodeIdPair(nodeId, leftBinInfoT, leftBinInfoV, leftPrediction, rightBinInfoT, rightBinInfoV, rightPrediction);

                        var leftNodeId = pair.Item1;
                        var rightNodeId = pair.Item2;
                        var leftNode = ConstructTreeNode(leftNodeId, currentTreeNode, false, leftIndicator, leftPrediction);
                        var rightNode = ConstructTreeNode(rightNodeId, currentTreeNode, false, rightIndicator, rightPrediction);
                        tree.Grow(nodeId, leftNode, rightNode, null);
                    }
                    else
                    {
                        var orphanPrediction = split.OrphanPrediction;
                        var triple = pool.GenerateNodeIdTriple(nodeId, leftBinInfoT, leftBinInfoV, leftPrediction, rightBinInfoT, rightBinInfoV,
                            rightPrediction, orphanBinInfoT, orphanBinInfoV, orphanPrediction);

                        var leftNodeId = triple.Item1;
                        var rightNodeId = triple.Item2;
                        var orphanNodeId = triple.Item3;
                        var leftNode = ConstructTreeNode(leftNodeId, currentTreeNode, false, leftIndicator, leftPrediction);
                        var rightNode = ConstructTreeNode(rightNodeId, currentTreeNode, false, rightIndicator, rightPrediction);
                        var orphanNode = ConstructTreeNode(orphanNodeId, currentTreeNode, true, null, orphanPrediction);
                        tree.Grow(nodeId, leftNode, rightNode, orphanNode);
                    }
                }
                else
                {
                    var orphanPrediction = split.OrphanPrediction;
                    var triple = pool.GenerateNodeIdTriple(nodeId, leftBinInfoT, leftBinInfoV, leftPrediction, rightBinInfoT, rightBinInfoV,
                            rightPrediction, orphanBinInfoT, orphanBinInfoV, orphanPrediction);

                    var leftNodeId = triple.Item1;
                    var rightNodeId = triple.Item2;
                    var orphanNodeId = triple.Item3;
                    var leftNode = ConstructTreeNode(leftNodeId, currentTreeNode, false, leftIndicator, leftPrediction);
                    var rightNode = ConstructTreeNode(rightNodeId, currentTreeNode, false, rightIndicator, rightPrediction);
                    var orphanNode = ConstructTreeNode(orphanNodeId, currentTreeNode, true, null, orphanPrediction);
                    tree.Grow(nodeId, leftNode, rightNode, orphanNode);
                }
            }
        }

        private GbmTreeNode ConstructTreeNode(int id, GbmTreeNode parent, bool isOrphan, IndicatorFunction func, double pred)
        {
            var node = new GbmTreeNode(id, parent, isOrphan) { Indicator = func, Prediction = pred };
            return node;
        }

        private void UpdateNodeIndices(IList<int> selectedNodes, IDictionary<int, SplitInfo> splits, GbmTree tree)
        {
            var dict = selectedNodes.ToDictionary(nodeId => nodeId, nodeId => new List<int>(_nodeIndices.Length));
            for (var i = 0; i < _nodeIndices.Length; i++)
            {
                var nodeId = _nodeIndices[i];
                if (!dict.ContainsKey(nodeId)) continue;
                dict[nodeId].Add(i);
            }
            foreach (var pair in dict)
            {
                var nodeId = pair.Key;
                var indices = pair.Value;
                var split = splits[nodeId];
                var featureName = split.Feature;
                var array = _frame.GetIntegerArray(featureName);
                var node = tree.GetNode(nodeId);
                foreach (var idx in indices)
                {
                    var val = array[idx];
                    var child = node.Navigate(val);
                    _nodeIndices[idx] = child.Id;
                }
            }
        }

    }
}
