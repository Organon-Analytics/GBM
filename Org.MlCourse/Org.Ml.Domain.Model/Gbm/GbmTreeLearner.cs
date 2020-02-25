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
        #region External fields
        private GbmAlgorithmSettings _algorithmSettings;
        private ModellingDataSettings _dataSettings;
        private DataFrame _frame;
        #endregion

        #region Helper fields
        private double[] _weights;
        private int[] _nodeIndices;
        private string[] _eligibleInputColumns;
        private IBlas _blas;
        #endregion

        private const int RootNodeIdx = -1;
        public GbmTreeLearner(GbmAlgorithmSettings algorithmSettings, ModellingDataSettings dataSettings, DataFrame frame)
        {
            _algorithmSettings = algorithmSettings;
            _dataSettings = dataSettings;
            _frame = frame;
            _blas = new DotNetBlas();
        }

        /// <summary>
        /// Initialize some internal fields to be used in training the model
        /// </summary>
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

        /// <summary>
        /// A Gbm model is a summation of individual (weak) models
        /// Each individual model is a C&R tree for this implementation
        /// GbmTree object holds all the necessary information about the tree created at a certain iteration
        /// The inputs to the Train() method are gradient and hessian vectors AT this iteration
        /// that the gradient and hessian vectırs are created once and updated after each iteration
        /// </summary>
        /// <param name="gradients"></param>
        /// <param name="hessians"></param>
        /// <returns></returns>
        public GbmTree Train(double[] gradients, double[] hessians, double[] delta)
        {
            //Randomize rows
            _frame.RandomizeTrainingIndices(_algorithmSettings.RowSamplingRate);
            //Randomize features
            var sampledFeatures = _frame.GetRandomInputList(_eligibleInputColumns, _algorithmSettings.ColumnSamplingRate);
            //Get pointers to training and validation indices
            var sampledTrainingIndices = _frame.GetRandomTrainingIndices();
            var validationIndices = _frame.GetValidationIndices();
            
            // Initialize BinInfo  with the entire training data for this iteration. 
            // This will be used in the while loop and it is a constant
            var rootBinInfoTraining = new BinInfo();
            foreach (var idx in sampledTrainingIndices)
            {
                rootBinInfoTraining.Add(1, gradients[idx], hessians[idx]);
            }
            // Initialize BinInfo  with the entire validation data. 
            // This will be used in the while loop and it is a constant
            var rootBinInfoValidation = new BinInfo();
            foreach (var idx in validationIndices)
            {
                rootBinInfoValidation.Add(1, gradients[idx], hessians[idx]);
            }

            // Initialize a GbmTreeNodePool object
            // NodePool keeps track of information about nodes as they are created:
            // 1 Keeps track of small-node, and large node for each pair created
            // 2 Keep track of BinInfo per node(for training and validation)
            // 3 Keeps track of alive and dead nodes
            // 4 Keeps track of the SplitInfo per node
            // 5 Keeps track of the prediction per node
            var nodePool = new GbmTreeNodePool(RootNodeIdx, rootBinInfoTraining, rootBinInfoValidation);

            // _nodeIndices data structure keeps the node-id for each row in the data as stored in _frame(DataFrame) object
            // These indices need to be updated whenever a new pair of nodes are created
            // ResetNodeIndices method sets the node indices to a single value (-1)
            ResetNodeIndices(RootNodeIdx);

            // Create an empty GbmTree. The primary goal of Train(.) method is to fill out this object and return it
            var tree = GbmTree.CreateEmptyTree(RootNodeIdx);

            // A SplitComputer computes the best split and produces a SplitInfo objects
            // A SplitComputer object is created per feature and this object is used several times in the while loop below
            // Since it is a heavy object, it is efficient to create it once, and re-populate its data structures when needed
            var splitComputers = new Dictionary<string, SplitComputer>();
            foreach (var feature in sampledFeatures)
            {
                var bin = _frame.GetBin(feature);
                splitComputers.Add(feature, new SplitComputer(feature, bin.NumberOfBins, bin.BinType));
            }

            // These data structures will be used to compress the data that will be scanned per iteration
            // Remember: 
            // 1 Only small child will be scanned for a pair of children
            // 2 Inactive nodes will be skipped
            // Rows are sampled per iteration. Non-sampled rows needs to be skipped
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
                // Remember that GbmTreeNodePool keeps track of the ancillary data during tree creation
                // Each iteration in the while loop spawns the children created at the relevant level of the tree
                // E.g., at level-0 at most 2 children node are created, at level-1 4 nodes are created at most, and so on
                // The local nodeStatusBeforeSplit object copies the alive node identifiers from the GbmTreeNodePool object
                // The alive-dead states must be updated somewhere in the while loop below
                var nodeStatusBeforeSplit = nodePool.DeepCopyLifeStatus();

                // The local object splits hold the SplitInfo inofmration per node created at this level in the tree
                // An empty SplitInfo is created per node, and filled out with output per feature per node
                var splits = new Dictionary<int, SplitInfo>();
                foreach (var item in nodeStatusBeforeSplit)
                {
                    if (item.Value)
                    {
                        splits.Add(item.Key, new SplitInfo());
                    }
                }

                // This computation is a tricky one. Note that the computations are NOT carried out over each row
                // Specifically, running the Aggregate() method of of splitComputer objects need to be computed only for small nodes
                // The respective data structures are automatically updated with the Complete() method of splitComputer object 
                // UpdateOrderedData method (repeatedly) fills out the data structures in its arguments with the relevant data
                // It returns lengthTraining as the output
                // This is the last index that needs to be processed in each data structure as input to the UpdateOrderedData method
                var lengthTraining = UpdateOrderedData(smallNodes, sampledTrainingIndices, orderedRowIndicesT, _nodeIndices,
                    orderedNodeIndicesT, _weights, orderedWeightsT, gradients, orderedGradientsT, hessians,
                    orderedHessiansT, 0);

                // For loop inside the while loop
                // The other while loop fixes the tree level
                // Eat each iteration of the for loop, the SplitComputer object per feature is updated for ALL the active nodes
                // Henc, as the program exits the for loop, the best splits are computed per feature per active-node
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
                // GetNodesToBeSplit() updates the new active nodes
                // Remember: This info is stored in nodePool object
                // If the number of total leaves exceeds MaxLeaves, or no node could be spawned, the while loop breaks
                var selectedNodes = GetNodesToBeSplit(splits, nodePool, _algorithmSettings.MaxLeaves);
                var isSuccess = (selectedNodes.Count > 0);
                // Break the while loop. One of the termination tules have been breached
                if (!isSuccess) break;

                // This is yet another heavy method
                // It updates the nodePool data structures
                // It update the GbmTree (basically grıws the tree and fills out GbmTree data structures
                UpdateTreeAndNodePool(selectedNodes, splits, nodePool, tree);

                // Gets and sorts small nodes
                smallNodes = nodeRelations.Keys.ToList();
                smallNodes.Sort();
                // This is a critical method as well. 
                // All the node indices as stored in _nodeIndices structure must be refreshed
                UpdateNodeIndices(selectedNodes, splits, tree);
                // If the number of leaf nodes exceed the MaxLeaves parameter, while loop is broken
                var numLeafNodes = tree.GetNumLeafNodes();
                // Break the while loop
                if (numLeafNodes >= _algorithmSettings.MaxLeaves) break;
            }
            ComputeDelta(0, _nodeIndices, nodePool.PredictionByNode, delta);

            return !tree.IsEmpty() ? tree : null;
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

        private unsafe void ComputeDelta(int offset, int[] nodeIndices, IDictionary<int, double> predictions, double[] delta)
        {
            fixed (double* dP = delta)
            {
                var pD = dP + offset;
                for (var i = 0; i < nodeIndices.Length; i++)
                {
                    *pD++ = predictions[nodeIndices[i]];
                }
            }
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
