using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.BlockchainComponents;

namespace SimpleBlockchain.Mining
{
    public interface IMiningFactory
    {
        IMiner GetMiner(IEnumerable<Transaction> transactions, Block previousBlock);
        IHashFactory GetMiningHashFactoryForTransactions(IEnumerable<Transaction> transactions, Block previousBlock);
        IHashFactory GetMiningHashFactoryById(int id);
        IRewarder GetRewarder();
    }
}
