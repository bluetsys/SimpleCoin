using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.WalletComponents
{
    public interface IWalletManager
    {
        Wallet AddNewWallet();
        IEnumerable<Wallet> GetWallets();
        void AcceptTransactions(IEnumerable<Transaction> transactions);
    }
}
