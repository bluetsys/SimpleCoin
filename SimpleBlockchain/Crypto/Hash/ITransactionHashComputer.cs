using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.WalletComponents;

namespace SimpleBlockchain.Crypto.Hash
{
    public interface ITransactionHashComputer
    {
        byte[] GetHashForTransactions(IEnumerable<Transaction> transactions);
    }
}
