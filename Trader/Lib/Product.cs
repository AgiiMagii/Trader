using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Trader.Lib.Enums;

namespace Trader.Lib
{
    public class Product
    {
        public Product(bool setUpDefaults)
        {
            if (setUpDefaults)
                SetUpDefaults();
        }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Freshness Freshness { get; set; }
        public int TicksToExpire { get; set; }
        public string ImagePath => GetEnumDescription(Type);
        public Products Type { get; set; }
        public int MaxDemand { get; set; } = int.MaxValue;
        public void SetUpDefaults()
        {
            Random rand = new Random();
            var values = Enum.GetValues(typeof(Products));
            Type = (Products)values.GetValue(rand.Next(0, values.Length));
            Name = Type.ToString();
            Price = Math.Round((decimal)(10 + rand.NextDouble() * 90), 2);
            Quantity = rand.Next(1, 10);
            Freshness = Freshness.Fresh;
            TicksToExpire = rand.Next(2, 4);
            MaxDemand = rand.Next(1, 10);
        }
        public static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }

    }
}
