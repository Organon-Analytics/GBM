using System;
using System.Collections.Generic;
using System.Text;

namespace Org.Infrastructure.Data
{
    public class TreeNodePool
    {
        private readonly IDictionary<int, BinInfo> _trainingBinInfo;

        public BinInfo GetBinInfoTraining(int nodeId)
        {
            return _trainingBinInfo[nodeId];
        }
    }
}
