using Org.Infrastructure.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    [Serializable]
    public class GbmTreeNode
    {
        private int _id;
        private GbmTreeNode _parent;
        private bool _isOrphan;

        private GbmTreeNodeType _nodeType;
        private int _depth;

        private GbmTreeNode _left;
        private GbmTreeNode _right;
        private GbmTreeNode _orphan;
        private Func<int, GbmTreeNode> _navigator;

        private GbmTreeNode()
        {
            _id = -1;
            _parent = null;
            _isOrphan = false;
            _nodeType = GbmTreeNodeType.Interim;
            _depth = -1;
            _left = null;
            _right = null;
            _orphan = null;
            _navigator = null;
        }
        public GbmTreeNode(int id, GbmTreeNode parent, bool isOrphan)
        {
            _id = id;
            _parent = parent;
            _isOrphan = isOrphan;
            if (parent == null)
            {
                _nodeType = GbmTreeNodeType.Interim;
            }
            else
            {
                _nodeType = _isOrphan ? GbmTreeNodeType.Orphan : GbmTreeNodeType.Leaf;
            }
            _depth = (parent == null) ? 0 : (1 + _parent.Depth);
            _left = null;
            _right = null;
            _orphan = null;
            _navigator = null;
        }


        public int Id { get { return _id; } set { _id = value; } }
        public bool IsOrphan { get { return _isOrphan; } set { _isOrphan = value; } }
        public GbmTreeNodeType NodeType { get { return _nodeType; } set { _nodeType = value; } }
        public int Depth { get { return _depth; } set { _depth = value; } }
        public string SplitFeature { get; set; }
        public IndicatorFunction Indicator { get; set; }
        public Func<int, GbmTreeNode> Navigator { get { return _navigator; } set { _navigator = value; } }
        public double Prediction { get; set; }
        public GbmTreeNode ParentNode { get { return _parent; } set { _parent = value; } }
        public GbmTreeNode LeftNode { get { return _left; } set { _left = value; } }
        public GbmTreeNode RightNode { get { return _right; } set { _right = value; } }
        public GbmTreeNode OrphanNode { get { return _orphan; } set { _orphan = value; } }

        public void Grow(GbmTreeNode left, GbmTreeNode right, GbmTreeNode orphan = null)
        {
            _left = left;
            _right = right;
            _orphan = orphan;
            if (_orphan == null)
            {
                _navigator = NavigateTwo;
            }
            else
            {
                _navigator = NavigateThree;
            }
            _nodeType = GbmTreeNodeType.Interim;
        }
        public void Prune()
        {
            _left = null;
            _right = null;
            _orphan = null;
        }
        public GbmTreeNode Navigate(int val)
        {
            return _navigator(val);
        }
        public void Scale(double alpha)
        {
            if (alpha > 0.0)
            {
                Prediction = alpha * Prediction;
            }
        }
        private GbmTreeNode NavigateTwo(int val)
        {
            return _left.Indicator.Contains(val) ? _left : _right;
        }
        private GbmTreeNode NavigateThree(int val)
        {
            if (_left.Indicator.Contains(val))
            {
                return _left;
            }
            if (_right.Indicator.Contains(val))
            {
                return _right;
            }
            return _orphan;
        }

        public static GbmTreeNode Clone(GbmTreeNode root)
        {
            if (root == null) return null;
            var node = new GbmTreeNode
            {
                Id = root.Id,
                ParentNode = root.ParentNode,
                IsOrphan = root.IsOrphan,
                NodeType = root.NodeType,
                Depth = root.Depth,
                SplitFeature = root.SplitFeature,
                Prediction = root.Prediction,
                Navigator = root.Navigator
            };
            if (root.Indicator != null)
            {
                node.Indicator = root.Indicator.Clone();
            }
            if (root.LeftNode != null)
            {
                node.LeftNode = Clone(root.LeftNode);
                node.RightNode = Clone(root.RightNode);
            }
            if (root.OrphanNode != null)
            {
                node.OrphanNode = Clone(root.OrphanNode);
            }
            return node;
        }
    }
}
