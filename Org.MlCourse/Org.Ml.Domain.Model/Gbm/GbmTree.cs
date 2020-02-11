using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    [Serializable]
    public class GbmTree
    {
        private readonly GbmTreeNode _root;
        private Dictionary<int, GbmTreeNode> _nodeById;
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

        public int GetNumLeafNodes()
        {
            return _nodeById.Select(kv => kv.Value).Count(node => node.NodeType == GbmTreeNodeType.Leaf);
        }

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

        public void Scale(double alpha)
        {
            Scale(_root, alpha);
        }

        private void Scale(GbmTreeNode node, double alpha)
        {
            if (node == null) return;
            node.Scale(alpha);
            if (node.NodeType == GbmTreeNodeType.Leaf) return;
            Scale(node.LeftNode, alpha);
            Scale(node.RightNode, alpha);
            Scale(node.OrphanNode, alpha);
        }

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

        //public string ToSql(Dictionary<string, Bin> bins, SampleSettings sampleSettings)
        //{
        //    var sql = ToSql(bins);
        //    var builder = new StringBuilder();
        //    builder.AppendLine("SELECT");
        //    builder.AppendLine(sql);
        //    builder.AppendLine("AS SUB");
        //    builder.AppendLine("FROM");
        //    switch (sampleSettings.SampleCreationMethod)
        //    {
        //        case SampleCreationMethod.TableSelect:
        //            builder.AppendLine(String.Format("{0}", sampleSettings.ResourceLocator));
        //            break;
        //        case SampleCreationMethod.FreehandSql:
        //            builder.AppendLine("(");
        //            builder.AppendLine(sampleSettings.ResourceLocator.ToSelectStatement());
        //            builder.AppendLine(") TIN");
        //            break;
        //        default:
        //            throw new InvalidOperationException("Sql could not be run on a text file");
        //    }
        //    return builder.ToString();
        //}

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
