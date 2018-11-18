using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleBlockchain.BlockchainComponents;
using SimpleBlockchain.Crypto.Hash;

namespace SimpleBlockchain.Mining
{
    public class BasicMiner : IMiner
    {
        public int Difficulty { get; }
        public IHashFactory HashFactory { get; }

        public BasicMiner(int difficulty, IHashFactory hashFactory)
        {
            Difficulty = difficulty;
            HashFactory = hashFactory;
        }

        public void MineBlock(Block block)
        {
            INonceGenerator nonceGenerator = HashFactory.GetNonceGenerator();
            IByteConverter converter = HashFactory.GetByteConverter();

            if (Difficulty >= HashFactory.GetDigest().HashLength)
                throw new ArgumentException("Difficulty can not be greater or equal to hash length.");

            if (Difficulty <= 0)
                throw new ArgumentException("Difficulty can not be greater or equal to zero.");

            byte[] target = new byte[Difficulty];

            block.Difficulty = Difficulty;
            nonceGenerator.Reset();

            #region Parallel mining.

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            object locker = new object();
            byte[] solutionNonce = null;
            byte[] solutionHash = null;

            string immutableData = converter.ConvertToString(block.PreviousHash) +
                                   block.CreationTime.ToString() +
                                   converter.ConvertToString(block.MerkleRoot) +
                                   converter.ConvertToString(block.MinerAddress) +
                                   block.HashAlgorithmId +
                                   block.Difficulty;
            try
            {
                Parallel.ForEach(nonceGenerator, new ParallelOptions() { CancellationToken = tokenSource.Token }, (byte[] nonce) =>
                {
                    IDigest digest = HashFactory.GetDigest();

                    converter = HashFactory.GetByteConverter();

                    string dataWithNonce = converter.ConvertToString(nonce) + immutableData;
                    byte[] rawData = Encoding.ASCII.GetBytes(dataWithNonce);
                    byte[] hash = digest.GetHash(rawData);

                    if (hash.Take(Difficulty).SequenceEqual(target))
                    {
                        lock (locker)
                        {
                            solutionNonce = nonce;
                            solutionHash = hash;
                        }

                        tokenSource.Cancel();
                    }
                });
            }
            catch (OperationCanceledException)
            {

            }

            block.Nonce = solutionNonce ?? throw new MiningFailureException("Couldn't resolve block", block);
            block.Hash = solutionHash;

            #endregion
        }
    }
}
