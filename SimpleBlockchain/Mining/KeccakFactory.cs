using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.Crypto.Hash;

namespace SimpleBlockchain.Mining
{
    class KeccakFactory : IHashFactory
    {
        public int Id { get; }
        public int HashLength { get; }
        
        private Lazy<KeccakMerkleRootComputer> keccakMerkleRoot;
        private Lazy<BigIntegerNonceGenerator> nonceGenerator;

        public KeccakFactory(int id, int hashLengthInBits, int nonceLength)
        {
            Id = id;
            HashLength = hashLengthInBits / 8;

            keccakMerkleRoot = new Lazy<KeccakMerkleRootComputer>(() => new KeccakMerkleRootComputer(512));
            nonceGenerator = new Lazy<BigIntegerNonceGenerator>(() => new BigIntegerNonceGenerator(nonceLength));
        }

        public IByteConverter GetByteConverter() => new ByteConverter();

        public IDigest GetDigest() => new KeccakDigest(Id, HashLength * 8);

        public INonceGenerator GetNonceGenerator() => nonceGenerator.Value;

        public ITransactionHashComputer GetTransactionHashComputer() => keccakMerkleRoot.Value;
    }
}
