using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class CityEvent
    {
        public string EventName { get; set; }
        public string Description { get; set; }
        public double BalanceChange { get; set; }
        public double Probability { get; set; }
        public bool IsRegularEvent { get; set; }
        public double MinBalanceThreshold { get; set; }
    }
}
