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
            [Description("/Assets/Images/Cheese.png")]
            Cheese,
            [Description("/Assets/Images/Fish.png")]
            Fish,
            [Description("/Assets/Images/MapleSyrup.png")]
            MapleSyrup,
            [Description("/Assets/Images/BreadPretzel.png")]
            BreadPretzel,
            [Description("/Assets/Images/CocaCola.png")]
            CocaCola,
            [Description("/Assets/Images/Pomegranate.png")]
            Pomegranate,
            [Description("/Assets/Images/Orange.png")]
            Orange,
            [Description("/Assets/Images/Cherry.png")]
            Cherry,
            [Description("/Assets/Images/Lemon.png")]
            Lemon,
            [Description("/Assets/Images/Passionfruit.png")]
            Passionfruit,
            [Description("/Assets/Images/Watermelon.png")]
            Watermelon,
            [Description("/Assets/Images/Strawberry.png")]
            Strawberry
        }
        public enum Freshness
        {
            Expired,
            Normal,
            Fresh,
            Rotten
        }
        public enum MessageType
        {
            Info,
            Success,
            Warning,
            Error
        }
    }
}
