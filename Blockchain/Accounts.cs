using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain
{
    class Accounts
    {
        private Dictionary<string, float> accounts = new Dictionary<string, float>();

        public float getBalance(string key)
        {
            return accounts[key];
        }

        //TODO: Add function to loop through blockchain and update balances for each pubkey
    }
}
