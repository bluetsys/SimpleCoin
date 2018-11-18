using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Configs.Parameters
{
    public class BasicMiningFactoryParameters
    {
        public double HashLeastUpFactor { get; set; }
        public double HashMostUpFactor { get; set; }

        public double HashLeastDownFactor { get; set; }
        public double HashMostDownFactor { get; set; }

        public double DifficultyLeastUpFactor { get; set; }
        public double DifficultyMostUpFactor { get; set; }

        public double DifficultyLeastDownFactor { get; set; }
        public double DifficultyMostDownFactor { get; set; }

        public int MinDifficulty { get; set; }
    }
}
