using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.WalletComponents;

namespace SimpleBlockchain.Mining
{
    class BasicRewarder : IRewarder
    {
        private int amountSum(IEnumerable<Transaction> transactions) => transactions.Sum(transaction => transaction.Amount);

        public int GetRewardForBlock(Block block)
        {
            int sum = amountSum(block.Transactions);
            int reward = (int)((10 + block.HashAlgorithmId * 3) / 100.0 * sum);

            return reward;
        }
    }
}
