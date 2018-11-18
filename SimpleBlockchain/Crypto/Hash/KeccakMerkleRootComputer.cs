using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MerkleTree;
using SimpleBlockchain.WalletComponents;
using SHA3;

namespace SimpleBlockchain.Crypto.Hash
{
    public class KeccakMerkleRootComputer : ITransactionHashComputer
    {
        private SHA3Managed digest;
        private MerkleTreeBuilder merkleBuilder;

        public KeccakMerkleRootComputer(int hashLengthInBits)
        {
            merkleBuilder = new MerkleTreeBuilder();
            digest = new SHA3Managed(hashLengthInBits);
        }

        public byte[] GetHashForTransactions(IEnumerable<Transaction> transactions)
        {
            IEnumerable<byte[]> hashes = transactions.Select(transaction => transaction.Hash);

            return merkleBuilder.GetMerkleRoot(hashes, digest);
        }
    }
}
