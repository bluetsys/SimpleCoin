using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;

namespace SimpleBlockchain.WalletComponents
{
    public class Wallet
    {
        public byte[] PublicKey { get; }
        public Blockchain Blockchain { get; set; }

        public int NumberOfTokens { get; private set; }
        
        public ISignatureProvider Signer { get; }

        public IHashFactory HashFactory { get; set; }

        public Wallet(ISignatureProvider signatureProvider, IHashFactory hashFactory)
        {
            HashFactory = hashFactory;

            Signer = signatureProvider;

            PublicKey = Signer.PublicKey;
            NumberOfTokens = 0;
        }

        public void AcceptTransactions(IEnumerable<Transaction> transactions) => NumberOfTokens += transactions.Where(tr => tr.Recipient.SequenceEqual(PublicKey)).Sum(tr => tr.Amount);

        public void SendTokens(int amount, byte[] recipient)
        {
            Transaction transaction = new Transaction(PublicKey, recipient, amount, HashFactory);

            transaction.SignTransaction(Signer);

            try
            {
                Blockchain.AddNewTransaction(transaction);
                NumberOfTokens -= amount;
            }
            catch (ArgumentException)
            {
                NumberOfTokens += amount;
            }
        }
    }
}
