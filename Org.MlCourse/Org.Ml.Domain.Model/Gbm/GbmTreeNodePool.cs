using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Ml.Domain.Model.Gbm
{
    public class GbmTreeNodePool
    {
        private readonly IDictionary<int, KeyValuePair<int, int>> _nodeRelations; // key: -1, value: <0, 1>
        private readonly IDictionary<int, BinInfo> _trainingBinInfo;
        private readonly IDictionary<int, BinInfo> _validationBinInfo;
        private readonly IDictionary<int, bool> _nodeLifeStatus;
        private readonly IDictionary<int, SplitInfo> _splitByNode;
        private readonly IDictionary<int, double> _predictionByNode;
        private int _maxNodeId;
        private readonly object _thisLock = new object();

        public GbmTreeNodePool(int rootNodeId, BinInfo rootBinInfoTraining, BinInfo rootBinInfoValidation)
        {
            _nodeRelations = new Dictionary<int, KeyValuePair<int, int>>();
            _trainingBinInfo = new Dictionary<int, BinInfo>
            {
                { rootNodeId, rootBinInfoTraining }
            };
            _validationBinInfo = new Dictionary<int, BinInfo>
            {
                { rootNodeId, rootBinInfoValidation }
            };
            _nodeLifeStatus = new Dictionary<int, bool>
            {
                { rootNodeId, true }
            };
            _splitByNode = new Dictionary<int, SplitInfo>
            {
                { rootNodeId, new SplitInfo() }
            };
            _predictionByNode = new Dictionary<int, double>
            {
                { rootNodeId, Double.NaN }
            };
            _maxNodeId = rootNodeId;
        }

        public IDictionary<int, bool> NodeLifeStatus
        {
            get { return _nodeLifeStatus; }
        }

        public IDictionary<int, SplitInfo> SplitByNode
        {
            get { return _splitByNode; }
        }

        public IDictionary<int, double> PredictionByNode
        {
            get { return _predictionByNode; }
        }

        public IDictionary<int, KeyValuePair<int, int>> NodeRelations
        {
            get { return _nodeRelations; }
        }

        public IDictionary<int, bool> DeepCopyLifeStatus()
        {
            return _nodeLifeStatus.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public void ClearNodeRelations()
        {
            _nodeRelations.Clear();
        }

        public Tuple<int, int> GenerateNodeIdPair(int parentId, BinInfo leftInfoT, BinInfo leftInfoV, double leftPrediction,
                                                                BinInfo rightInfoT, BinInfo rightInfoV, double rightPrediction)
        {
            lock (_thisLock)
            {
                var leftId = ++_maxNodeId;
                var rightId = ++_maxNodeId;
                var leftIsSmaller = (leftInfoT.SumWeights <= rightInfoT.SumWeights);
                Update(leftId, rightId, parentId, leftInfoT, leftInfoV, leftPrediction, leftIsSmaller);
                Update(rightId, leftId, parentId, rightInfoT, rightInfoV, rightPrediction, !leftIsSmaller);
                _nodeLifeStatus[parentId] = false;
                return new Tuple<int, int>(leftId, rightId);
            }
        }

        public Tuple<int, int, int> GenerateNodeIdTriple(int parentId, BinInfo leftInfoT, BinInfo leftInfoV, double leftPrediction,
                                                                       BinInfo rightInfoT, BinInfo rightInfoV, double rightPrediction,
                                                                       BinInfo orphanInfo, BinInfo orphanInfoV, double orphanPrediction)
        {
            lock (_thisLock)
            {
                var leftId = ++_maxNodeId;
                var rightId = ++_maxNodeId;
                var orphanId = ++_maxNodeId;
                var leftIsSmaller = (leftInfoT.SumWeights <= rightInfoT.SumWeights);
                Update(leftId, rightId, parentId, leftInfoT, leftInfoV, leftPrediction, leftIsSmaller);
                Update(rightId, leftId, parentId, rightInfoT, rightInfoV, rightPrediction, !leftIsSmaller);
                UpdateOrphan(orphanId, orphanInfo, orphanInfoV, orphanPrediction);
                _nodeLifeStatus[parentId] = false;
                return new Tuple<int, int, int>(leftId, rightId, orphanId);
            }
        }

        private void UpdateOrphan(int nodeId, BinInfo binInfoT, BinInfo binInfoV, double prediction)
        {
            _trainingBinInfo.Add(nodeId, binInfoT);
            _validationBinInfo.Add(nodeId, binInfoV);
            _nodeLifeStatus.Add(nodeId, false);
            _predictionByNode.Add(nodeId, prediction);
        }

        private void Update(int nodeId, int pairId, int parentId, BinInfo binInfoT, BinInfo binInfoV, double prediction, bool isSmaller)
        {
            _trainingBinInfo.Add(nodeId, binInfoT);
            _validationBinInfo.Add(nodeId, binInfoV);
            _nodeLifeStatus.Add(nodeId, true);
            _predictionByNode.Add(nodeId, prediction);

            _splitByNode.Add(nodeId, new SplitInfo());
            if (isSmaller)
            {
                _nodeRelations.Add(nodeId, new KeyValuePair<int, int>(pairId, parentId));
            }
        }

        public BinInfo GetBinInfoTraining(int nodeId)
        {
            return _trainingBinInfo[nodeId];
        }

        public BinInfo GetBinInfoValidation(int nodeId)
        {
            return _validationBinInfo[nodeId];
        }
    }
}
