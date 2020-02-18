using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    [Serializable]
    public class SumOfTrees
    {
        private readonly List<GbmTree> _forest;

        public SumOfTrees()
        {
            _forest = new List<GbmTree>();
        }

        public IList<GbmTree> Forest { get { return _forest; } }

        public void Add(GbmTree tree)
        {
            _forest.Add(tree);
        }

        public string ToSql(Dictionary<string, Bin> binCollection)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < _forest.Count; i++)
            {
                var tree = _forest[i];
                builder.AppendLine(tree.ToSql(binCollection));
                if (i < (_forest.Count - 1))
                {
                    builder.AppendLine("+");
                }
            }
            return builder.ToString();
        }

        public SumOfTrees Clone()
        {
            var result = new SumOfTrees();
            foreach (var tree in _forest)
            {
                result.Add(tree.Clone());
            }
            return result;
        }

        public IList<string> GetAllFeatures()
        {
            var list = new List<string>();
            foreach (var tree in _forest)
            {
                var newFeatures = tree.GetTreeFeatures();
                list = list.Union(newFeatures).ToList();
            }
            return list;
        }
    }
}
