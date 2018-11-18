using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;

namespace SimpleBlockchain.BlockchainComponents
{
    public class Block
    {
        public byte[] MinerAddress { get; set; }
        public byte[] MinerSignature { get; set; }
        public int Difficulty { get; set; }
        public int HashAlgorithmId { get; set; }

        public ICollection<Transaction> Transactions { get; set; }

        public byte[] Nonce { get; set; }
        public byte[] PreviousHash { get; set; }
        public byte[] Hash { get; set; }
        public byte[] MerkleRoot { get; set; }

        public DateTime CreationTime { get; set; }

        public Block()
        { }

        public Block(byte[] minerAddress, byte[] previousHash, ICollection<Transaction> transactions, IHashFactory hashFactory)
        {
            MinerAddress = minerAddress;

            Transactions = transactions;
            CreationTime = DateTime.Now;

            IDigest digest = hashFactory.GetDigest();
            INonceGenerator nonceGenerator = hashFactory.GetNonceGenerator();

            PreviousHash = new byte[previousHash.Length];
            Array.Copy(previousHash, PreviousHash, previousHash.Length);

            MerkleRoot = ComputeMerkleRoot(hashFactory);
            Nonce = nonceGenerator.GetNextNonce();
            Hash = ComputeHash(hashFactory);
        }

        public byte[] ComputeMerkleRoot(IHashFactory hashFactory)
        {
            ITransactionHashComputer merkleRootComputer = hashFactory.GetTransactionHashComputer();

            return merkleRootComputer.GetHashForTransactions(Transactions);
        }
        
        public byte[] ComputeHash(IHashFactory hashFactory)
        {
            IByteConverter converter = hashFactory.GetByteConverter();
            IDigest digest = hashFactory.GetDigest();

            HashAlgorithmId = digest.Id;

            string data = converter.ConvertToString(Nonce) +
                          converter.ConvertToString(PreviousHash) +
                          CreationTime.ToString() +
                          converter.ConvertToString(MerkleRoot) +
                          converter.ConvertToString(MinerAddress) +
                          HashAlgorithmId +
                          Difficulty;

            byte[] rawData = Encoding.ASCII.GetBytes(data);

            return digest.GetHash(rawData);
        }

        public void SignBlock(ISignatureProvider signer) => MinerSignature = signer.SignHash(Hash);

        public bool VerifyBlockSignature(ISignatureVerifier verifier) => verifier.VerifyHash(MinerAddress, Hash, MinerSignature);
    }
}
