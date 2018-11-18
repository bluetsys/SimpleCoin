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
    public class JsonUnitRepositoryConfig : IUnitRepositoryConfig
    {
        private UnitRepositoryParameters parameters;

        public string DirectoryPath => parameters.DirectoryPath;

        public JsonUnitRepositoryConfig(string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            using (Stream jsonFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            using (StreamReader reader = new StreamReader(jsonFile))
            using (JsonReader jsonReader = new JsonTextReader(reader))
                parameters = jsonSerializer.Deserialize<UnitRepositoryParameters>(jsonReader);
        }
    }
}
