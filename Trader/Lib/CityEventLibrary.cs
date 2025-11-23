using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class CityEventLibrary
    {
        public static List<CityEvent> GetCityEvents()
        {
            return new List<CityEvent>
            {
                new CityEvent
                {
                    EventName = "Road Costs",
                    Description = "Road costs -20 €",
                    BalanceChange = -20.0,
                    Probability = 1.0,
                    MinBalanceThreshold = 500.0,
                    IsRegularEvent = true
                },
                new CityEvent
                {
                    EventName = "Car Accident",
                    Description = "You were involved in a car accident. -200 €",
                    BalanceChange = -200.0,
                    Probability = 0.05,
                    MinBalanceThreshold = 500.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Hitch",
                    Description = "You hitch a ride with a fellow traveler. +10 €",
                    BalanceChange = 10.0,
                    Probability = 0.6,
                    MinBalanceThreshold = 0.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Natural Disaster",
                    Description = "A natural disaster has struck, causing significant damage to your assets. -400 €",
                    BalanceChange = -400.0,
                    Probability = 0.05,
                    MinBalanceThreshold = 1000.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Found Money",
                    Description = "You found some money on the street. +50 €",
                    BalanceChange = 50.0,
                    Probability = 0.10,
                    MinBalanceThreshold = 0.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Lost Wallet",
                    Description = "You lost your wallet while traveling. -100 €",
                    BalanceChange = -100.0,
                    Probability = 0.10,
                    MinBalanceThreshold = 500.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Market Fair",
                    Description = "You participated in the fair and earned a good profit. +1000 €",
                    BalanceChange = 1000.0,
                    Probability = 0.05,
                    MinBalanceThreshold = 2000.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Charity Donation",
                    Description = "You decided to donate to a local charity. -75 €",
                    BalanceChange = -75.0,
                    Probability = 0.10,
                    MinBalanceThreshold = 300.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Robbery",
                    Description = "You were robbed while traveling between cities. -1000 €",
                    BalanceChange = -1000.0,
                    Probability = 0.05,
                    MinBalanceThreshold = 3000.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Lucky Find",
                    Description = "You stumbled upon a hidden treasure. +500 €",
                    BalanceChange = 500.0,
                    Probability = 0.05,
                    MinBalanceThreshold = 0.0,
                    IsRegularEvent = false
                },
                new CityEvent
                {
                    EventName = "Travel Delay",
                    Description = "Your travel was delayed, incurring extra costs. -50 €",
                    BalanceChange = -50.0,
                    Probability = 0.10,
                    MinBalanceThreshold = 500.0,
                    IsRegularEvent = false
                }
            };
        }
    }
}
