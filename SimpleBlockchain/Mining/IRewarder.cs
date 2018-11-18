using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;

namespace SimpleBlockchain.Mining
{
    public interface IRewarder
    {
        int GetRewardForBlock(Block block);
    }
}
