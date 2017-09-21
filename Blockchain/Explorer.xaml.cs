using System;
using System.Linq;
using System.Windows;
using System.Timers;
using System.Windows.Threading;

namespace Blockchain
{
    /// <summary>
    /// Explorer for the SteedyBucks BlockChain
	/// Written by David Steedman 2017
    /// </summary>
    public partial class BlockChainExplorer : Window
    {
		int blockIndex = 0;
		int blockTotal = 0;
		static Timer timer;
		Node node = new Node();

		public BlockChainExplorer()
        {
            InitializeComponent();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			bool result = await node.LoadBlockchain();

			if (result)
			{				
				blockTotal = node.blockHeight - 1;
				blockIndex = blockTotal;
				ShowBlockInfo(blockIndex);

				if (blockIndex > 0)
				{
					PreviousButton.IsEnabled = true;
				}
			}

			timer = new Timer(1000);
			timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
			timer.Enabled = true;			
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			blockTotal = node.blockHeight - 1;
			if(blockIndex < blockTotal)
			{
				this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
				{
					NextButton.IsEnabled = true;
				}));				
			}
		}

		private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
			blockIndex--;
			ShowBlockInfo(blockIndex);
			if (blockIndex == 0)
			{
				PreviousButton.IsEnabled = false;
			}
			NextButton.IsEnabled = true;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
			blockIndex++;
			ShowBlockInfo(blockIndex);
			if (blockIndex == node.chain.Count()-1)
			{
				NextButton.IsEnabled = false;
			}
			PreviousButton.IsEnabled = true;
		}

		private void ShowBlockInfo(int index)
		{
			Tuple<string,string,string> result = node.queryBlockInfo(index);
			DataGridView.ItemsSource = node.chain.ElementAt(index).GetData().getAllTransactions();
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
			// Asks for private key to sign transaction
			string priv_key = Microsoft.VisualBasic.Interaction.InputBox("Enter your private key to sign this transaction.", "Private Key", "BwIAAACkAABSU0EyAAQAAAEAAQA/KlTgRpoq4gx6RQFiPz+1FAEq5VOUZrfOWJ593nwm7gjsV6x+uxEJRScSLOmBda2PEmVTlFim2Y/Aund29KDXryz16sl6719hR3qZVUkEohAbvPayDJ7r70EAM+oGAU8KiPnyoQqF6bLexEE9yXtA39q94KTPMC4wT7jIhi1E9Xv4KnkASrOC7ZPfBytKFpdYDFd1pPbrGQz9kq7Lc0HIp9l0JMWPPCU0np/NLqSiIma3jPV5hYQOS9s0QRY92v0N5AgQNeY3khyqMLwvvm80HMOoBObsqUsw6BkTOpIttvWc6Vs9bgSCh7KWQZnD0fQFdVRH3pEiXEkMBuAoWFf3a7Jng9ZYToQGtk7o3fq90IxkL+L9HWlCFh4v6rmgmGYOdcgUAll7oR4J3lc85nviw5m8V7lc4/RJZMMkUFq9R13OILvuJU9yx6OFn2NK77SC7mtGgzSwY+rTlq8JFrq+n/NPpdZQmUWKb4D17N5w3dOfOY08vQCAhtanByJFKBRKCPaAJ0VgjZl1T1U7KGeSlYfJ8fsb7ug6giWX+OLFSoOQmMLoCvJKeRwHMEQ4qoJcdnhmx9t/TY3E0djuOWgtXdoF7PZgowjLpu/S9gq1gJPPQ2Db1R4UFHITBS+xmsp5cmelFKJW0ZgS4DCHvoCMyNDJkyLvtKISVb85Hq7YBwMM2l7H6hDlk7zjgYPqEQPbnVWYJn/5F65VTvuQfsi21LiptEzT+ksBU2m3fLFJ/GfpnxT2+rSGIQm4xzRs2Bs=");
			string signature = Keys.createSig(senderTextBox.Text, recipientTextBox.Text, double.Parse(amountTextBox.Text),priv_key);

			// Attempts to add transaction to current block on node
			Tuple<bool, string> result = node.AddTransaction(senderTextBox.Text, recipientTextBox.Text, double.Parse(amountTextBox.Text),signature);
			MessageBox.Show(result.Item2);

			// Clears the input boxes
			senderTextBox.Clear();
            recipientTextBox.Clear();
            amountTextBox.Clear();
		}

		private void GetBalance_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(Node.getBalance(PubKeyGetBalanceTextBox.Text).ToString() + " confirmed. " + node.getUnconfirmedBalance(PubKeyGetBalanceTextBox.Text).ToString() + " unconfirmed.");
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			System.Windows.Application.Current.Shutdown();
		}

		private void LaunchMinerButton_Click(object sender, RoutedEventArgs e)
		{
			if (node.mw.IsVisible)
			{
				node.mw.Hide();
				node.mining = false;
				LaunchMinerButton.Content = "Start Mining";
			}
			else
			{
				node.mw.Show();
				node.mining = true;
				node.MineBlock();
				LaunchMinerButton.Content = "Stop Mining";
			}			
		}
	}
}
