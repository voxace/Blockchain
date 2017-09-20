namespace Blockchain
{
	public class Transaction
	{
		public string sender
		{
			get;
			set;
		}
		public string recipient
		{
			get;
			set;
		}
		public double amount
		{
			get;
			set;
		}
		public string txid
		{
			get;
			set;
		}

		public Transaction(string _sender, string _recipient, double _amount, string _txid)
		{
			sender = _sender;
			recipient = _recipient;
			amount = _amount;
			txid = _txid;
		}

		public string GetTransactionString()
		{
			return sender + "," + recipient + "," + amount.ToString() + "," + txid + "\n";
		}
	}
}
