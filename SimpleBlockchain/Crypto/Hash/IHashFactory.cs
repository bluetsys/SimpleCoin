using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Hash
{
    public interface IHashFactory
    {
        IByteConverter GetByteConverter();
        IDigest GetDigest();
        INonceGenerator GetNonceGenerator();
        ITransactionHashComputer GetTransactionHashComputer();
    }
}
