using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Storage
{
    public interface IBlockchainUnit : IBlockStorage
    {
        byte[] GetUnitHash();
    }
}
