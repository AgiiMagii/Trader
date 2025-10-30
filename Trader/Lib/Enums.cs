using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class Enums
    {
        public enum Products
        {
            [Description("/Controls/Cheese.png")]
            Cheese,
            [Description("/Controls/Fish.png")]
            Fish,
            [Description("/Controls/MapleSyrup.png")]
            MapleSyrup,
            [Description("/Controls/BreadPretzel.png")]
            BreadPretzel,
            [Description("/Controls/CocaCola.png")]
            CocaCola,
            [Description("/Controls/Pomegranate.png")]
            Pomegranate,
            [Description("/Controls/Orange.png")]
            Orange,
            [Description("/Controls/Cherry.png")]
            Cherry,
            [Description("/Controls/Lemon.png")]
            Lemon,
            [Description("/Controls/Passionfruit.png")]
            Passionfruit,
            [Description("/Controls/Watermelon.png")]
            Watermelon,
            [Description("/Controls/Strawberry.png")]
            Strawberry
        }
        public enum Freshness
        {
            Expired,
            Normal,
            Fresh,
            Rotten
        }
    }
}
