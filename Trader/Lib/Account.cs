using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class Account
    {
        private double Balance { get; set; }

        public Account(double balance = 100.00)
        {
            Balance = balance;
        }
        public void IncreaseBalance(double amount)
        {
            Balance += amount;
        }
        public void DecreaseBalance(double amount)
        {
            if (Balance >= amount)
            {
                Balance -= amount;
            }
        }
        public double GetBalance()
        {
            return Balance;
        }
    }
}
