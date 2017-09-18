using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Blockchain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		int blockIndex = 0;
		Node node = new Node();

		public MainWindow()
        {
            InitializeComponent();            
			showBlockInfo(blockIndex);

			// Testing asymmetric signature verification
			string messageToSign = "Hello World!";
			string signedMessage = Keys.SignData(messageToSign, Ledger.steedy_private_key);
			MessageBox.Show(signedMessage);
			bool success = Keys.VerifyData(messageToSign, signedMessage, Ledger.steedy_pub_key);
			MessageBox.Show("Is this message sent by me? " + success);

		}        

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
			blockIndex--;
			showBlockInfo(blockIndex);
			if (blockIndex == 0)
			{
				PreviousButton.IsEnabled = false;
			}
			NextButton.IsEnabled = true;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
			blockIndex++;
			showBlockInfo(blockIndex);
			if (blockIndex == node.chain.Count()-1)
			{
				NextButton.IsEnabled = false;
			}
			PreviousButton.IsEnabled = true;
		}

		private void showBlockInfo(int index)
		{
			Tuple<string,string,string> result = node.queryBlockInfo(index);
			dataTextBox.Text = result.Item1;
			timestampTextBox.Text = result.Item2;
			previousHashTextBox.Text = result.Item3;
			indexTextBox.Text = index.ToString();
		}

		private void createKeyPairButton_Click(object sender, RoutedEventArgs e)
		{
			Tuple<string, string> keypair = Keys.CreateKeyPair();
			privKeyText.Text = keypair.Item1;
			pubKeyText.Text = keypair.Item2;
		}

		private void AddDataButton_Click(object sender, RoutedEventArgs e)
		{
			Tuple<bool, Transaction> result = Node.SendTransaction(senderTextBox.Text, recipientTextBox.Text, double.Parse(amountTextBox.Text),"");
			if(result.Item1)
			{
				node.newLedger.addTransaction(result.Item2);
			}
			else
			{
				MessageBox.Show("Insufficient Funds");
			}			

			senderTextBox.Clear();
            recipientTextBox.Clear();
            amountTextBox.Clear();
		}

        private async void AddBlock()
        {
			// Move this logic into Miner.cs
            Serialize.WriteBlock(node.chain.ElementAt(node.chain.Count() - 1));         
            Block newBlock = new Block();
            newBlock.newBlock(node.chain.Count(), DateTime.Now, node.newLedger, node.chain.ElementAt(node.chain.Count() - 1).HashBlock());
			addBlockButton.IsEnabled = false;
			if (await node.chain.ElementAt(node.chain.Count() - 1).mineBlock(true))
			{
				// Actually add block when we receive broadcast (not just by clicking this button!)
				node.chain.Add(newBlock);
			}
			addBlockButton.IsEnabled = true;
			node.newLedger = new Ledger();
            NextButton.IsEnabled = true;
        }

        private void addBlockButton_Click(object sender, RoutedEventArgs e)
        {
            AddBlock();
        }

		private void GetBalance_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(Node.getBalance(PubKeyGetBalanceTextBox.Text).ToString());
		}
	}
}
