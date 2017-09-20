using System.Collections.Generic;

namespace Blockchain
{
    public class Ledger
    {        
        public List<Transaction> transactions = new List<Transaction>();

        public Ledger()
        {

        }

        public void AddTransaction(Transaction t)
        {
            transactions.Add(t);
        }

		public void removeTransaction(Transaction t)
		{
			transactions.Remove(t);
		}

        public string getString()
        {
            string data = "";
            foreach (Transaction t in transactions)
            {
                data += t.GetTransactionString();
            }
            return data;
        }

		public double getBalance(string pubKey)
		{
			double balance = 0;

			foreach (Transaction t in transactions)
			{
				if(t.recipient == pubKey)
				{
					balance += t.amount;
				}
				if(t.sender == pubKey)
				{
					balance -= t.amount;
				}
			}
			return balance;
		}

		public List<Transaction> getAllTransactions()
		{
			return transactions;
		}

	}

    
}
