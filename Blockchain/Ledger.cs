using System.Collections.Generic;

namespace Blockchain
{
    public class Ledger
    {        
        public List<Transaction> transactions = new List<Transaction>();

        public Ledger()
        {

        }

        public void AddTransaction(Transaction transaction)
        {
			bool match = false;
			foreach (Transaction t in transactions)
			{				
				if(t.txid == transaction.txid)
				{
					// Transaction alredy exists in current ledger, don't add
					match = true;
				}
			}
			if(match == false)
			{
				// Transaction is new, add to ledger
				transactions.Add(transaction);
			}            
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
