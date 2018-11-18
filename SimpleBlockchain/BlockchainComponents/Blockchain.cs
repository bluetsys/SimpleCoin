using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleBlockchain.WalletComponents;
using SimpleBlockchain.Net;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;
using SimpleBlockchain.Mining;
using SimpleBlockchain.Storage;

namespace SimpleBlockchain.BlockchainComponents
{
    public class Blockchain : IEnumerable<Block>
    {
        public Wallet MiningWallet { get; set; }
        public IWalletManager WalletManager { get; set; }

        private IBlockStorage storedChain;
        private LinkedList<Transaction> pendingTransactions;

        public ISignatureFactory SignatureFactory { get; set; }
        public ISignatureVerifier Verifier { get; }

        public IHashFactory TransactionHashFactory { get; set; }
        public IDigest Digest { get; }

        public IMiningFactory MiningFactory { get; set; }

        public IBroadcaster NetworkBroadcaster { get; set; }
        
        public bool IsValid
        {
            get
            {
                Block firstBlock = storedChain.First();
                byte[] previousHash = new byte[firstBlock.PreviousHash.Length];
                DateTime previousTimeStamp = new DateTime(1, 1, 1);
                bool genesisTrigger = true;

                Array.Copy(firstBlock.PreviousHash, previousHash, firstBlock.PreviousHash.Length);

                foreach (Block block in storedChain)
                {
                    if (!genesisTrigger)
                        try
                        {
                            validateBlock(block, previousHash, previousTimeStamp);
                        }
                        catch
                        {
                            return false;
                        }

                    genesisTrigger = false;
                    previousHash = new byte[block.Hash.Length];

                    Array.Copy(block.Hash, previousHash, block.Hash.Length);
                    previousTimeStamp = block.CreationTime;
                }

                return true;
            }
        }

        public Blockchain(Wallet miningWallet, IWalletManager walletManager, IHashFactory transactionHashFactory, ISignatureFactory signatureFactory, IMiningFactory miningFactory, IBlockStorage storage)
        {
            SignatureFactory = signatureFactory;
            Verifier = signatureFactory.GetSignatureVerifier();

            MiningFactory = miningFactory;

            TransactionHashFactory = transactionHashFactory;
            Digest = TransactionHashFactory.GetDigest();

            MiningWallet = miningWallet;
            WalletManager = walletManager;

            pendingTransactions = new LinkedList<Transaction>();
            storedChain = storage;

            foreach (Block block in storedChain)
                WalletManager.AcceptTransactions(block.Transactions);
        }

        private Block addBlock()
        {
            Block lastBlock = storedChain.Last;
            IHashFactory hashFactory = MiningFactory.GetMiningHashFactoryForTransactions(pendingTransactions, lastBlock);
            Block block = new Block(MiningWallet.PublicKey, lastBlock.Hash, pendingTransactions, hashFactory);
            IMiner miner = MiningFactory.GetMiner(block.Transactions, lastBlock);

            miner.MineBlock(block);
            block.SignBlock(MiningWallet.Signer);

            storedChain.AddBlock(block);
            WalletManager.AcceptTransactions(block.Transactions);

            return block;
        }

        #region Transaction validation.

        private bool validateTransactionAmount(Transaction transaction, DateTime transactionTime) => GetNearestIncomesForTransaction(transaction, transactionTime) >= transaction.Amount;

        private bool validateTransactionSignature(Transaction transaction) => transaction.VerifyTransactionSignature(Verifier);

        private bool validateTransactionHash(Transaction transaction) => transaction.Hash.SequenceEqual(transaction.ComputeHash(TransactionHashFactory));

        #endregion

        #region Block validation.

        private bool validateBlockHash(Block block)
        {
            IHashFactory hashFactory = MiningFactory.GetMiningHashFactoryById(block.HashAlgorithmId);

            return block.Hash.SequenceEqual(block.ComputeHash(hashFactory));
        }

        private bool validateBlockPreviousHash(Block block, byte[] previousHash) => block.PreviousHash.SequenceEqual(previousHash);

        private bool validateBlockMerkleRoot(Block block) => block.MerkleRoot.SequenceEqual(block.ComputeMerkleRoot(TransactionHashFactory));

        private bool validateBlockProofOfWork(Block block) => block.Hash.Take(block.Difficulty).SequenceEqual(new byte[block.Difficulty]);

        private void validateBlockTransactions(Block block)
        {
            foreach (Transaction transaction in block.Transactions)
                validateTransaction(transaction, block.CreationTime);
        }

        private bool validateBlockSignature(Block block) => block.VerifyBlockSignature(Verifier);

        private bool validateBlockTimestamp(Block block, DateTime previousTimestamp) => previousTimestamp < block.CreationTime;

        #endregion

        private void validateBlock(Block block, byte[] previousHash, DateTime previousTimestamp)
        {
            if (!validateBlockPreviousHash(block, previousHash))
                throw new ArgumentException("Hash of previous block is incorrect for this block");

            if (!validateBlockMerkleRoot(block))
                throw new ArgumentException("Merkle root of the block is invalid");

            if (!validateBlockHash(block))
                throw new ArgumentException("Hash of the block is invalid");

            if (!validateBlockProofOfWork(block))
                throw new ArgumentException("The block is not mined");

            validateBlockTransactions(block);

            if (!validateBlockSignature(block))
                throw new ArgumentException("Signature of the block is invalid");

            if (!validateBlockTimestamp(block, previousTimestamp))
                throw new ArgumentException("Timestamp of the block is invalid");
        }

        private void validateTransaction(Transaction transaction, DateTime transactionTime)
        {
            if (!validateTransactionHash(transaction))
                throw new ArgumentException("Transaction hash is invalid");

            if (!validateTransactionSignature(transaction))
                throw new ArgumentException("Transaction signature is invalid");

            if (!validateTransactionAmount(transaction, transactionTime))
                throw new ArgumentException("Sender doesn't have enough tokens on his cash");
        }

        public int CountBalanceForWallet(byte[] address)
        {
            int total = 0;

            foreach (Block block in storedChain)
                foreach (Transaction transaction in block.Transactions)
                {
                    if (transaction.Recipient.SequenceEqual(address))
                    {
                        total += transaction.Amount;

                        continue;
                    }

                    if (transaction.Sender.SequenceEqual(address) && !transaction.Recipient.SequenceEqual(address))
                        total -= transaction.Amount;
                }

            return total;
        }

        public int GetNearestIncomesForTransaction(Transaction transaction, DateTime transactionTime)
        {
            int total = 0;

            foreach (Block block in storedChain.GetBlocksReverse())
                if (block.CreationTime < transactionTime)
                    foreach (Transaction tr in block.Transactions)
                    {
                        if (tr.Recipient.SequenceEqual(transaction.Sender))
                        {
                            total += tr.Amount;

                            if (total >= transaction.Amount)
                                return total;

                            continue;
                        }

                        if (tr.Sender.SequenceEqual(transaction.Sender) && !tr.Recipient.SequenceEqual(transaction.Sender))
                            total -= tr.Amount;
                    }

            return total;
        }
        
        public void AcceptTransaction(Transaction transaction)
        {
            validateTransaction(transaction, DateTime.Now);
            pendingTransactions.AddLast(transaction);
        }
        
        public void AcceptBlock(Block block)
        {
            byte[] previousHash = storedChain.Last.Hash;

            validateBlock(block, previousHash, storedChain.Last.CreationTime);

            storedChain.AddBlock(block);
            WalletManager.AcceptTransactions(block.Transactions);
            pendingTransactions = new LinkedList<Transaction>();
        }

        public void AddNewTransaction(Transaction transaction)
        {
            AcceptTransaction(transaction);
            NetworkBroadcaster?.BroadcastTransaction(transaction);
        }

        public void ProcessPendingTransactions()
        {
            Block block = addBlock();

            NetworkBroadcaster?.BroadcastBlock(block);
            pendingTransactions = new LinkedList<Transaction>();

            IRewarder rewarder = MiningFactory.GetRewarder();
            int rewardValue = rewarder.GetRewardForBlock(block);
            Transaction reward = new Transaction(MiningWallet.PublicKey, MiningWallet.PublicKey, rewardValue, TransactionHashFactory);

            reward.SignTransaction(MiningWallet.Signer);
            AddNewTransaction(reward);
        }

        public IEnumerator<Block> GetEnumerator() => storedChain.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
