using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

			// Used in genesis block creation
			//pubKeyText.Text = Keys.createSig("BgIAAACkAABSU0ExAAQAAAEAAQDxfEKJ4dv9QwoEtoroufSZxXj8wR7P6ThRfD/kTm4InLRRWaZqy1OdW2RQoYwQhxIQG5rslOZhIIWPBhm3pTB89fS9L1lMwCLzNhq5bkoWE/rcgEckb5lGplRl1NIkjIPSHYLukWQToK0Tx7BKIA42npiufEcFORCwsp0uLnE9xw==",
			//								"BgIAAACkAABSU0ExAAQAAAEAAQA/KlTgRpoq4gx6RQFiPz+1FAEq5VOUZrfOWJ593nwm7gjsV6x+uxEJRScSLOmBda2PEmVTlFim2Y/Aund29KDXryz16sl6719hR3qZVUkEohAbvPayDJ7r70EAM+oGAU8KiPnyoQqF6bLexEE9yXtA39q94KTPMC4wT7jIhi1E9Q==",
			//								1000.0, "BwIAAACkAABSU0EyAAQAAAEAAQDxfEKJ4dv9QwoEtoroufSZxXj8wR7P6ThRfD/kTm4InLRRWaZqy1OdW2RQoYwQhxIQG5rslOZhIIWPBhm3pTB89fS9L1lMwCLzNhq5bkoWE/rcgEckb5lGplRl1NIkjIPSHYLukWQToK0Tx7BKIA42npiufEcFORCwsp0uLnE9xzeTJ/UoiLSQWyScwNJEtaKHrk2Yb8RMXbiCEPm65IkCGGtXGdolXRFlWP/AroKmeTInRKO3w1B0NwLRqQWGAdEXVUVXPfTBk80FMmYMjhj9GHDmtkW5BmpYdMsLbxEUeP67oTe69buuZraChqcosr5IvudhdF8yaeLwH0NZyQn0BfgisUqjAo9T/iVbQhzbNRDhgF53OmazL8VuUxFLraZx+DVntPhVu1BfzN+Aslt6vUtqvPjvC9yEet3oHnVMgH8sejN7ilCDUYReV3V6OvCWYYG4JBbsvdTvYe/mnnRYRKA2HQRYYxRubTLoqaWxGotk2bULTUCzanwfQZFJQdMwDyiTVdvf4AgUmKMcqtqph4E9anYEM9immplBXeadrr7RrlY52eTHIKeY7mbmZCOoR5PZ1Exyyk95RTYOZAtMcSwVwZE1CgI/Q8S13DgHAhgMYeb2i7ogFiJThGqIigU0QDzh2MNhGi6/F/AKSTcS8wadPuK6pNJfVuCMM4M892mmPnPZn+m/uCGi6gc9N4cRsWOeHJ00k3JlZc9Fh3LM4UkKGgx9KM//AAoHRadU9KUgoSI3mH4kcUp9xKFn1kU=");
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
			DataGridView.ItemsSource = node.chain.ElementAt(blockIndex).GetData().getAllTransactions();
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
			string signature = Keys.createSig(senderTextBox.Text, recipientTextBox.Text, double.Parse(amountTextBox.Text),Ledger.steedy_private_key);
			Tuple<bool, string> result = node.SendTransaction(senderTextBox.Text, recipientTextBox.Text, double.Parse(amountTextBox.Text),signature);
			if(!result.Item1)
			{
				MessageBox.Show(result.Item2);
			}		

			senderTextBox.Clear();
            recipientTextBox.Clear();
            amountTextBox.Clear();
		}

        private async void AddBlock()
        {
			// Move this logic into Miner.cs
			// Double check the logic on this, am I adding the correct block?
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
