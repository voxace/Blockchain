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
        Block currentBlock;
        List<Block> chain = new List<Block>();
        Ledger newLedger = new Ledger();
        List<Accounts> accounts = new List<Accounts>();

        public MainWindow()
        {
            InitializeComponent();
            createGenesisBlock();
            currentBlock = createGenesisBlock();
            chain.Add(currentBlock);
			showBlockInfo();
        }

        private Block createGenesisBlock()
        {
            // The Following lines show how the Genesis block was created originally:
                //Ledger genesisData = new Ledger();
                //genesisData.addTransaction(new Transaction("SteedyBucks", "Steedy", 1000000.0f));
                //Block genesis = new Block();
                //genesis.newBlock(0, DateTime.Now, genesisData, "0");
                //return genesis;
            
            // Reads the hard coded genesis block from JSON file
            return Serialize.ReadBlock("0000000000");
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
			currentBlock = chain.ElementAt(currentBlock.getIndex() - 1);
			showBlockInfo();
			if (currentBlock.getIndex() == 0)
			{
				PreviousButton.IsEnabled = false;
			}
			NextButton.IsEnabled = true;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
			currentBlock = chain.ElementAt(currentBlock.getIndex() + 1);
			showBlockInfo();
			if (currentBlock.getIndex() == chain.Count()-1)
			{
				NextButton.IsEnabled = false;
			}
			PreviousButton.IsEnabled = true;
		}

		private void showBlockInfo()
		{
			dataTextBox.Text = currentBlock.GetData().getString();
			timestampTextBox.Text = currentBlock.getTimestamp().ToUniversalTime().ToString();
			previousHashTextBox.Text = currentBlock.getPreviousHash();
			indexTextBox.Text = currentBlock.getIndex().ToString();
		}

		private void createKeyPairButton_Click(object sender, RoutedEventArgs e)
		{
			Tuple<string, string> keypair = Keys.CreateKeyPair();
			privKeyText.Text = keypair.Item1;
			pubKeyText.Text = keypair.Item2;
		}

		private void AddDataButton_Click(object sender, RoutedEventArgs e)
		{
            newLedger.addTransaction(new Blockchain.Transaction(senderTextBox.Text, recipientTextBox.Text, float.Parse(amountTextBox.Text)));
			senderTextBox.Clear();
            recipientTextBox.Clear();
            amountTextBox.Clear();
		}
        private void AddBlock()
        {
            MessageBox.Show(chain.ElementAt(chain.Count() - 1).index.ToString());
            Serialize.WriteBlock(chain.ElementAt(chain.Count() - 1));         
            Block newBlock = new Block();
            newBlock.newBlock(chain.Count(), DateTime.Now, newLedger, chain.ElementAt(chain.Count() - 1).HashBlock());
            chain.Add(newBlock);
            
            newLedger = new Ledger();
            NextButton.IsEnabled = true;
        }

        private void addBlockButton_Click(object sender, RoutedEventArgs e)
        {
            AddBlock();
        }
    }
}
