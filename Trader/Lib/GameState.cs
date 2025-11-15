using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class GameState
    {
        public double PlayerBalance { get; set; }
        public List<Product> Inventory { get; set; }
        public List<Product> CityOffers { get; set; }
        public List<Product> ToRemove { get; set; }
        public Dictionary<string, int> SoldThisSession { get; set; }
        public string CurrentCity { get; set; }
        public double BestScore { get; set; }

        public GameState()
        {
            PlayerBalance = 100;
            Inventory = new List<Product>();
            CityOffers = new List<Product>();
            ToRemove = new List<Product>();
            SoldThisSession = new Dictionary<string, int>();
            CurrentCity = "London";
            BestScore = 0;
        }
    }
}
