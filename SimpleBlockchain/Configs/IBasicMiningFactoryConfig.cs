using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Configs
{
    public interface IBasicMiningFactoryConfig
    {
        double HashLeastUpFactor { get; }
        double HashMostUpFactor { get; }

        double HashLeastDownFactor { get; }
        double HashMostDownFactor { get; }

        double DifficultyLeastUpFactor { get; }
        double DifficultyMostUpFactor { get; }

        double DifficultyLeastDownFactor { get; }
        double DifficultyMostDownFactor { get; }

        int MinDifficulty { get; }
    }
}
