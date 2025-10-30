using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class GameState
    {
        private static Random rand = new Random();

        public double PlayerBalance { get; set; }
        public List<Product> Inventory { get; set; }
        public List<Product> CityOffers { get; set; }
        public List<Product> ToRemove { get; set; }
        public Dictionary<string, int> SoldThisSession { get; set; }
        public string CurrentCity { get; set; }

        public GameState()
        {
            LoadDefaults();
        }
        public GameState(GameState existingState)
        {
            if (existingState != null)
            {
                //PlayerBalance = existingState.PlayerBalance;
                //Inventory = existingState.Inventory.Select(p => p.Clone()).ToList();
                //CityOffers = existingState.CityOffers.Select(p => p.Clone()).ToList();
                //ToRemove = existingState.ToRemove.Select(p => p.Clone()).ToList();
                //SoldThisSession = new Dictionary<string, int>(existingState.SoldThisSession);
                //CurrentCity = existingState.CurrentCity;
            }
            else
            {
                LoadDefaults();
            }
        }
        private void LoadDefaults()
        {
            PlayerBalance = 100;
            Inventory = new List<Product>();
            CityOffers = new List<Product>();
            ToRemove = new List<Product>();
            SoldThisSession = new Dictionary<string, int>();
            CurrentCity = "London";
        }
    }


}
