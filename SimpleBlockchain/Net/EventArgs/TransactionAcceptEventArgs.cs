using System;
using System.Collections.Generic;
using System.Text;
using SimpleBlockchain.WalletComponents;

namespace SimpleBlockchain.Net.EventArgs
{
    class TransactionAcceptEventArgs : System.EventArgs
    {
        public Transaction Transaction { get; }

        public TransactionAcceptEventArgs(Transaction transaction)
        {
            Transaction = transaction;
        }
    }
}
