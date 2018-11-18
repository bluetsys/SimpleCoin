using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.Configs;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.Crypto.Hash;
using Newtonsoft.Json;
using System.IO;

namespace SimpleBlockchain.WalletComponents
{
    public class WalletManager : IWalletManager
    {
        private IWalletManagerConfig config;
        private ICollection<Wallet> wallets;

        private ISignatureFactory signatureFactory;
        private IHashFactory hashFactory;

        public WalletManager(IWalletManagerConfig config, ISignatureFactory signatureFactory, IHashFactory hashFactory)
        {
            this.config = config;

            this.signatureFactory = signatureFactory;
            this.hashFactory = hashFactory;

            wallets = new LinkedList<Wallet>();

            foreach (string walletPath in Directory.GetFiles(config.WalletDirectoryPath, "*.wallet"))
            {
                JsonSerializer serializer = new JsonSerializer();
                byte[] walletBlob;

                using (Stream jsonFile = File.Open(walletPath, FileMode.Open, FileAccess.Read, FileShare.None))
                using (StreamReader reader = new StreamReader(jsonFile))
                using (JsonReader jsonReader = new JsonTextReader(reader))
                    walletBlob = serializer.Deserialize<byte[]>(jsonReader);

                ISignatureProvider signer = signatureFactory.GetSignatureProvider(walletBlob);
                Wallet wallet = new Wallet(signer, hashFactory);

                wallets.Add(wallet);
            }
        }

        public Wallet AddNewWallet()
        {
            ISignatureProvider signer = signatureFactory.GetSignatureProvider();
            Wallet wallet = new Wallet(signer, hashFactory);

            string walletName = Path.GetRandomFileName();
            byte[] walletBlob = signer.ExportKeyPairBlob();

            walletName = Path.ChangeExtension(walletName, "wallet");

            string walletPath = Path.Combine(config.WalletDirectoryPath, walletName);
            JsonSerializer serializer = new JsonSerializer();

            using (Stream jsonFile = File.Open(walletPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(jsonFile))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                serializer.Serialize(jsonWriter, walletBlob);

            wallets.Add(wallet);

            return wallet;
        }

        public IEnumerable<Wallet> GetWallets() => wallets;

        public void AcceptTransactions(IEnumerable<Transaction> transactions)
        {
            foreach (Wallet wallet in wallets)
                wallet.AcceptTransactions(transactions);
        }
    }
}
