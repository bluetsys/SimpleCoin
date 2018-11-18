using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using SimpleBlockchain.Configs.Parameters;

namespace SimpleBlockchain.Configs
{
    public class JsonBasicMiningFactoryConfig : IBasicMiningFactoryConfig
    {
        private BasicMiningFactoryParameters parameters;

        public double HashLeastUpFactor => parameters.HashLeastUpFactor;
        public double HashMostUpFactor => parameters.HashMostUpFactor;

        public double HashLeastDownFactor => parameters.HashLeastDownFactor;
        public double HashMostDownFactor => parameters.HashMostDownFactor;

        public double DifficultyLeastUpFactor => parameters.DifficultyLeastUpFactor;
        public double DifficultyMostUpFactor => parameters.DifficultyMostUpFactor;

        public double DifficultyLeastDownFactor => parameters.DifficultyLeastDownFactor;
        public double DifficultyMostDownFactor => parameters.DifficultyMostDownFactor;

        public int MinDifficulty => parameters.MinDifficulty;

        public JsonBasicMiningFactoryConfig(string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            using (Stream jsonFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            using (StreamReader reader = new StreamReader(jsonFile))
            using (JsonReader jsonReader = new JsonTextReader(reader))
                parameters = jsonSerializer.Deserialize<BasicMiningFactoryParameters>(jsonReader);
        }
    }
}
