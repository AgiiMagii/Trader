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
        public enum Cities
        {
            [Description("/Assets/Images/NewYork.png")]
            NewYork,
            [Description("/Assets/Images/London.png")]
            London,
            [Description("/Assets/Images/Tokyo.png")]
            Tokyo,
            [Description("/Assets/Images/Riga.png")]
            Riga,
            [Description("/Assets/Images/Paris.png")]
            Paris,
            [Description("/Assets/Images/Amsterdam.png")]
            Amsterdam,
            [Description("/Assets/Images/Berlin.png")]
            Berlin,
            [Description("/Assets/Images/Milan.png")]
            Milan,
            [Description("/Assets/Images/Sydney.png")]
            Sydney,
            [Description("/Assets/Images/Rome.png")]
            Rome
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
