using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.Crypto.Hash;

namespace SimpleBlockchain.WalletComponents
{
    public class TransactionHashFactory : IHashFactory
    {
        public const int NonceLength = 64;

        private Lazy<KeccakDigest> digest;
        private Lazy<RandomNonceGenerator> nonceGenerator;
        private Lazy<ByteConverter> byteConverter;
        private Lazy<KeccakMerkleRootComputer> keccakMerkleRoot;

        public TransactionHashFactory()
        {
            digest = new Lazy<KeccakDigest>(() => new KeccakDigest(hashLengthInBits: 512));
            nonceGenerator = new Lazy<RandomNonceGenerator>(() => new RandomNonceGenerator(NonceLength));
            byteConverter = new Lazy<ByteConverter>(() => new ByteConverter());
            keccakMerkleRoot = new Lazy<KeccakMerkleRootComputer>(() => new KeccakMerkleRootComputer(hashLengthInBits: 512));
        }

        public IByteConverter GetByteConverter() => byteConverter.Value;

        public IDigest GetDigest() => digest.Value;

        public INonceGenerator GetNonceGenerator() => nonceGenerator.Value;

        public ITransactionHashComputer GetTransactionHashComputer() => keccakMerkleRoot.Value;
    }
}
