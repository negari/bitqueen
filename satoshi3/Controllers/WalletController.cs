using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using System.Numerics;
using System.Text;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace satoshi3.Controllers
{
    public class WalletController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public Network _network
        {
            set { }
        }

        private Network _btc_network ;  // Backing store

        public Network BtcNetwork
        {
            get
            {
                return _btc_network;
            }
            set
            {
                _btc_network = value;
            }
        }

        void CreateBTCWallet()
        {
            Key privateKey = new Key(); //Create private key
            BitcoinSecret secret = privateKey.GetBitcoinSecret(BtcNetwork);
            BitcoinAddress address = privateKey.PubKey.GetAddress(BtcNetwork); //Get the public key, and derive the address on the Main network
        }


        decimal BtcBalance(string btc_address)
        {
            var client = new QBitNinjaClient(BtcNetwork);
            //            var ops = client.GetBalanceBetween(new BalanceSelector(BitcoinAddress.Create("mnbg2tVoCcfmGFE7Su8Kc9QZ6b2RHLCD52")), new BlockFeature(336410), new BlockFeature(336409)).Result;
            var balanceModel = client.GetBalance(new BitcoinPubKeyAddress(btc_address), unspentOnly: true).Result;
            if (balanceModel.Operations.Count == 0)
                throw new Exception("No coins to spend");
            var unspentCoins = new List<Coin>();
            foreach (var operation in balanceModel.Operations)
                unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
            var balance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
            return balance;
        }


        public uint256 get_transaction_id(string addr, decimal amount)
        {
            var client = new QBitNinjaClient(BtcNetwork);
            //            var ops = client.GetBalanceBetween(new BalanceSelector(BitcoinAddress.Create("mnbg2tVoCcfmGFE7Su8Kc9QZ6b2RHLCD52")), new BlockFeature(336410), new BlockFeature(336409)).Result;
            var balanceModel = client.GetBalance(new BitcoinPubKeyAddress(addr), unspentOnly: true).Result;
            if (balanceModel.Operations.Count == 0)
                throw new Exception("No coins to spend");
            foreach (var operation in balanceModel.Operations)
            {
                if ((operation.ReceivedCoins[0] as Coin).Amount.ToDecimal(MoneyUnit.BTC) >= amount)
                    return operation.TransactionId;

            }
            throw new Exception("Not enough balance");
        }


        public void send_btc(BitcoinSecret sender, BitcoinAddress receiver, decimal amount, decimal fee)
        {
            var network = sender.Network;
            var address = sender.GetAddress();

            Console.WriteLine(sender); // cN5YQMWV8y19ntovbsZSaeBxXaVPaK4n7vapp4V56CKx5LhrK2RS
            Console.WriteLine(address); // mkZzCmjAarnB31n5Ke6EZPbH64Cxexp3Jp
            var client = new QBitNinjaClient(network);
            var transactionId = get_transaction_id(sender.PubKeyHash.GetAddress(BtcNetwork).ToString(), amount);// uint256.Parse("18ea7047ac70bbc16f53587e4fc28f7f9a6bb9117c0a810e1e65e424852b40ca");
            var transactionResponse = client.GetTransaction(transactionId).Result;

            Console.WriteLine(transactionResponse.TransactionId); // 0acb6e97b228b838049ffbd528571c5e3edd003f0ca8ef61940166dc3081b78a
            Console.WriteLine(transactionResponse.Block.Confirmations); // 91
            var receivedCoins = transactionResponse.ReceivedCoins;
            OutPoint outPointToSpend = null;
            foreach (var coin in receivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == sender.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }
            if (outPointToSpend == null)
                throw new Exception("TxOut doesn't contain our ScriptPubKey");
            Console.WriteLine("We want to spend {0}. outpoint:", outPointToSpend.N + 1);

            var transaction = new NBitcoin.Transaction();
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });

            // How much you want to spend
            var hallOfTheMakersAmount = new Money(amount, MoneyUnit.BTC);

            // How much miner fee you want to pay
            /* Depending on the market price and
             * the currently advised mining fee,
             * you may consider to increase or decrease it.
             */
            var minerFee = new Money(fee, MoneyUnit.BTC);

            // How much you want to get back as change
            var txInAmount = (Money)receivedCoins[(int)outPointToSpend.N].Amount;
            var changeAmount = txInAmount - hallOfTheMakersAmount - minerFee;

            TxOut hallOfTheMakersTxOut = new TxOut()
            {
                Value = hallOfTheMakersAmount,
                ScriptPubKey = receiver.ScriptPubKey
            };

            TxOut changeTxOut = new TxOut()
            {
                Value = changeAmount,
                ScriptPubKey = sender.ScriptPubKey
            };

            transaction.Outputs.Add(hallOfTheMakersTxOut);
            transaction.Outputs.Add(changeTxOut);

            var message = "Long live NBitcoin and its makers!";
            var bytes = Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });
            transaction.Inputs[0].ScriptSig = sender.ScriptPubKey;

            transaction.Sign(sender, false);

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

            if (!broadcastResponse.Success)
            {
                Console.Error.WriteLine("ErrorCode: " + broadcastResponse.Error.ErrorCode);
                Console.Error.WriteLine("Error message: " + broadcastResponse.Error.Reason);
            }
            else
            {
                Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                Console.WriteLine(transaction.GetHash());
            }

        }



    }
}