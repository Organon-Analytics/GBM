using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    /// <summary>
    /// GbmTree holds all the necessary data produced as the result of tree-build information
    /// The Train() method the GbmTreeLearner object produces a single GbmTree per iteration
    /// GbmTree is a binary tree and it stores GbmTreeNode obects in a tree structure
    /// </summary>
    [Serializable]
    public class GbmTree
    {
        /// <summary>
        /// The _root node is the root of the tree
        /// _nodeById structure is used for "fast access" to the inidividual nodes as binary traversal takes more time
        /// </summary>
        private readonly GbmTreeNode _root;
        private Dictionary<int, GbmTreeNode> _nodeById;

        /// <summary>
        /// Initialize the tree with the root node
        /// </summary>
        /// <param name="root"></param>
        public GbmTree(GbmTreeNode root)
        {
            _root = root;
            _nodeById = new Dictionary<int, GbmTreeNode> { { _root.Id, _root } };
        }

        public Dictionary<int, GbmTreeNode> NodeById { get { return _nodeById; } set { _nodeById = value; } }
        public bool IsEmpty()
        {
            return (_root == null) || (_root.LeftNode == null) || (_root.RightNode == null);
        }

        /// <summary>
        /// Score() method is used for in-memory scoring
        /// It takes a "row" of data as stored in inputs object and generates the tree prediction
        /// Note that each node object stores its prediction. 
        /// Hence, all we nood is to navigate to the relevant node
        /// This ias delegated to the NAvigate() method of the GbmTreeNode object
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public double Score(IDictionary<string, int> inputs)
        {
            var node = _root;
            while (true)
            {
                if (node.NodeType == GbmTreeNodeType.Leaf)
                {
                    return node.Prediction;
                }
                var val = inputs[node.SplitFeature];
                node = node.Navigate(val);
            }
        }
        /// <summary>
        /// A helper method used by its consumers to return the number of leaf(terminal) nodes
        /// </summary>
        /// <returns></returns>
        public int GetNumLeafNodes()
        {
            return _nodeById.Select(kv => kv.Value).Count(node => node.NodeType == GbmTreeNodeType.Leaf);
        }

        /// <summary>
        /// A helper method that returns a data structure representing predictions per node-id
        /// </summary>
        /// <returns></returns>
        public IDictionary<int, double> GetLeafNodePredictions()
        {
            var dict = new Dictionary<int, double>();
            foreach (var pair in _nodeById)
            {
                var nodeId = pair.Key;
                var node = pair.Value;
                if (node.NodeType == GbmTreeNodeType.Leaf)
                {
                    dict.Add(nodeId, node.Prediction);
                }
            }
            return dict;
        }

        /// <summary>
        /// Remember that we multiply the result of each tree prediction by the "learning rate"
        /// So, we have to scale it. 
        /// This method does it
        /// </summary>
        /// <param name="alpha"></param>
        public void Scale(double alpha)
        {
            Scale(_root, alpha);
        }

        /// <summary>
        /// This is the private method that does the actual scaling work in a recursive manner
        /// </summary>
        /// <param name="node"></param>
        /// <param name="alpha"></param>
        private void Scale(GbmTreeNode node, double alpha)
        {
            if (node == null) return;
            node.Scale(alpha);
            if (node.NodeType == GbmTreeNodeType.Leaf) return;
            Scale(node.LeftNode, alpha);
            Scale(node.RightNode, alpha);
            Scale(node.OrphanNode, alpha);
        }

        /// <summary>
        /// GbmTreeNode objects are created externally in the Train() method of the GbmTreeLearner object
        /// This method uses those objects to update its internal data structures
        /// </summary>
        /// <param name="baseNodeId"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="orphan"></param>
        public void Grow(int baseNodeId, GbmTreeNode left, GbmTreeNode right, GbmTreeNode orphan)
        {
            var node = GetNode(baseNodeId);
            node.Grow(left, right, orphan);
            _nodeById.Add(left.Id, left);
            _nodeById.Add(right.Id, right);
            if (orphan != null)
            {
                _nodeById.Add(orphan.Id, orphan);
            }
        }

        /// <summary>
        /// Helper method to access the tree nodes by node identifier
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public GbmTreeNode GetNode(int nodeId)
        {
            return _nodeById[nodeId];
        }

        public IList<string> GetTreeFeatures()
        {
            var list = new List<string>();
            GetTreeFeatures(list, _root);
            return list;
        }

        private void GetTreeFeatures(IList<string> list, GbmTreeNode node)
        {
            if (node == null) return;
            if (node.Indicator != null)
            {
                var feature = node.Indicator.Feature;
                if (!list.Contains(feature))
                {
                    list.Add(feature);
                }
            }
            GetTreeFeatures(list, node.LeftNode);
            GetTreeFeatures(list, node.RightNode);
        }

        public string ToSql(Dictionary<string, Bin> bins)
        {
            var builder = new StringBuilder();
            ToSql(builder, "  ", _root, bins);
            return builder.ToString();
        }

        /// <summary>
        /// This method is a recursive one and hard to grasp as such at the first glance
        /// It builds the SQL formula for the entire tree
        /// Note that each node know how to create its own SQL expression
        /// Hence it is enough here to organize the recursion part
        /// The left is delegated to the nodes
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="indent"></param>
        /// <param name="node"></param>
        /// <param name="bins"></param>
        public void ToSql(StringBuilder builder, string indent, GbmTreeNode node, Dictionary<string, Bin> bins)
        {
            var space = GetIndentation(node.Depth, indent);
            if (node.LeftNode == null || node.RightNode == null)
            {
                builder.AppendLine(String.Format("{0}{1}", space, node.Prediction));
                return;
            }
            var leftNode = node.LeftNode;
            var rightNode = node.RightNode;
            var orphanNode = node.OrphanNode;

            var feature = leftNode.Indicator.Feature;
            var bin = bins[feature];
            var leftPredicate = leftNode.Indicator.ToSql(bin);
            var rightPredicate = rightNode.Indicator.ToSql(bin);

            if (orphanNode == null)
            {
                builder.AppendLine(String.Format("{0}CASE WHEN {1} THEN", space, leftPredicate));
                ToSql(builder, indent, leftNode, bins);
                builder.AppendLine(String.Format("{0}ELSE", space));
                ToSql(builder, indent, rightNode, bins);
                builder.AppendLine(String.Format("{0}END", space));
            }
            else
            {
                var orphanPrediction = orphanNode.Prediction;
                builder.AppendLine(String.Format("{0}CASE WHEN {1} THEN", space, leftPredicate));
                ToSql(builder, indent, leftNode, bins);
                builder.AppendLine(String.Format("{0}WHEN {1} THEN", space, rightPredicate));
                ToSql(builder, indent, rightNode, bins);
                builder.AppendLine(String.Format("{0}ELSE {1} END", space, orphanPrediction));
            }
        }

        public string GetIndentation(int depth, string indentationToken)
        {
            var s = String.Empty;
            for (var i = 0; i < depth; i++)
            {
                s += indentationToken;
            }
            return s;
        }

        public static GbmTree CreateEmptyTree(int rootNodeId)
        {
            var rootNode = new GbmTreeNode(rootNodeId, null, false);
            return new GbmTree(rootNode);
        }

        public GbmTree Clone()
        {
            var newRoot = GbmTreeNode.Clone(_root);
            var newTree = new GbmTree(newRoot);
            newTree.NodeById.Clear();
            foreach (var pair in _nodeById)
            {
                newTree.NodeById.Add(pair.Key, pair.Value);
            }
            return newTree;
        }

    }
}
