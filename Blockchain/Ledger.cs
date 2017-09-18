using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain
{
    public class Ledger
    {
        private const int startingBalance = 1000000;
        public const string steedy_private_key = "BwIAAACkAABSU0EyAAQAAAEAAQA/KlTgRpoq4gx6RQFiPz+1FAEq5VOUZrfOWJ593nwm7gjsV6x+uxEJRScSLOmBda2PEmVTlFim2Y/Aund29KDXryz16sl6719hR3qZVUkEohAbvPayDJ7r70EAM+oGAU8KiPnyoQqF6bLexEE9yXtA39q94KTPMC4wT7jIhi1E9Xv4KnkASrOC7ZPfBytKFpdYDFd1pPbrGQz9kq7Lc0HIp9l0JMWPPCU0np/NLqSiIma3jPV5hYQOS9s0QRY92v0N5AgQNeY3khyqMLwvvm80HMOoBObsqUsw6BkTOpIttvWc6Vs9bgSCh7KWQZnD0fQFdVRH3pEiXEkMBuAoWFf3a7Jng9ZYToQGtk7o3fq90IxkL+L9HWlCFh4v6rmgmGYOdcgUAll7oR4J3lc85nviw5m8V7lc4/RJZMMkUFq9R13OILvuJU9yx6OFn2NK77SC7mtGgzSwY+rTlq8JFrq+n/NPpdZQmUWKb4D17N5w3dOfOY08vQCAhtanByJFKBRKCPaAJ0VgjZl1T1U7KGeSlYfJ8fsb7ug6giWX+OLFSoOQmMLoCvJKeRwHMEQ4qoJcdnhmx9t/TY3E0djuOWgtXdoF7PZgowjLpu/S9gq1gJPPQ2Db1R4UFHITBS+xmsp5cmelFKJW0ZgS4DCHvoCMyNDJkyLvtKISVb85Hq7YBwMM2l7H6hDlk7zjgYPqEQPbnVWYJn/5F65VTvuQfsi21LiptEzT+ksBU2m3fLFJ/GfpnxT2+rSGIQm4xzRs2Bs=";
        public const string steedy_pub_key = "BgIAAACkAABSU0ExAAQAAAEAAQA/KlTgRpoq4gx6RQFiPz+1FAEq5VOUZrfOWJ593nwm7gjsV6x+uxEJRScSLOmBda2PEmVTlFim2Y/Aund29KDXryz16sl6719hR3qZVUkEohAbvPayDJ7r70EAM+oGAU8KiPnyoQqF6bLexEE9yXtA39q94KTPMC4wT7jIhi1E9Q==";
        public List<Transaction> transactions = new List<Transaction>();

        public Ledger()
        {

        }

        public void addTransaction(Transaction t)
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
                data += t.getTransaction();
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

    }

    public class Transaction
    {
        public string sender;
        public string recipient;
        public double amount;

        public Transaction(string s, string r, double a)
        {
            sender = s;
            recipient = r;
            amount = a;
        }

		public string getTransaction()
        {
            return sender + "," + recipient + "," + amount.ToString() + "\n"; 
        }
    }
}
