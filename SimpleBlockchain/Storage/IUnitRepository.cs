using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SimpleBlockchain.Storage
{
    public interface IUnitRepository : IDisposable
    {
        BigInteger Count { get; }
        BigInteger GlobalCount { get; }
        IBlockchainUnit Last { get; }

        IEnumerable<IBlockchainUnit> GetUnits();
        IEnumerable<IBlockchainUnit> GetUnitsWithoutLast();
        IEnumerable<IBlockchainUnit> GetUnitsReverse();
        IEnumerable<IBlockchainUnit> GetUnitsReverseWithoutLast();
        IBlockchainUnit CreateNewUnit();
    }
}
