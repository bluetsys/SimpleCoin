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
    public class JsonWalletManagerConfig : IWalletManagerConfig
    {
        private WalletManagerParameters parameters;

        public string WalletDirectoryPath => parameters.WalletDirectoryPath;

        public JsonWalletManagerConfig(string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();

            using (Stream jsonFile = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            using (StreamReader reader = new StreamReader(jsonFile))
            using (JsonReader jsonReader = new JsonTextReader(reader))
                parameters = jsonSerializer.Deserialize<WalletManagerParameters>(jsonReader);
        }
    }
}
