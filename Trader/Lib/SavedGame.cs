using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class SavedGame
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FilePath { get; set; }
        public GameState State { get; set; }
    }
}
