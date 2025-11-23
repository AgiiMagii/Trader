using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Trader.Lib;
using static Trader.Lib.Enums;

namespace Trader.Controls
{
    public partial class GameControl : UserControl
    {
        public event Action GameOverTriggered;
        public event Action BackToMainMenuRequested;
        public event Action SavedGamesRequested;
        public string CurrentSaveFilePath { get; set; }
        private MessageService _messageService;
        private string gameName = "";
        private double newScore = 0;
        Account account = new Account();
        Random random = new Random();
        private List<Product> inventory = new List<Product>();
        private List<Product> cityOffers = new List<Product>();
        private List<Product> toRemove = new List<Product>();
        private BestScoreService bestScoreService = new BestScoreService();
        private GameState CurrentState;
        public GameControl(GameState initialState = null)
        {
            InitializeComponent();
            _messageService = new MessageService(MessagePanel);
            if (initialState != null)
                CurrentState = initialState;
            else
                CurrentState = new GameState();

            UpdateUI();
        }
        public void GameControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateBalance();
        }
        public void UpdateBalance()
        {
            BalanceBlock.Text = $"Balance: {account.GetBalance():F2}€";
        }
        private void GetNewBestScore()
        {
            if (account.GetBalance() > CurrentState.BestScore)
            {
                CurrentState.BestScore = account.GetBalance();
            }
            BestScoreBlock.Text = $"Best Score: {CurrentState.BestScore:F2}€";
            SaveBestScoreInfo();
            _messageService.ShowMessage("New Best Score!", MessageType.Success);
        }
        public void SaveBestScoreInfo()
        {
            gameName = System.IO.Path.GetFileNameWithoutExtension(CurrentSaveFilePath);
            newScore = account.GetBalance();
            bestScoreService.UpdateBestScore(gameName, newScore);
        }
        private void CityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                cityOffers.Clear();
                CityLabel.Content = button.Name;

                GenerateCityOffer();

                for (int i = 0; i < OfferGrid.Children.Count; i++)
                {
                    if (OfferGrid.Children[i] is Button offerBtn)
                    {
                        if (i < cityOffers.Count)
                        {
                            Product offerProduct = cityOffers[i];
                            offerBtn.IsEnabled = offerProduct.Quantity > 0;
                            UpdateOfferButtonUI(offerBtn, offerProduct);
                        }
                    }
                }
                ChangeConditionAndTick();
            }
            txtMessage.Text = "";
            _messageService.ShowMessage($"You traveled to {CityLabel.Content}.", MessageType.Info);
            CheckPlayerBalance();
        }
        private void UpdateOfferButtonUI(Button btn, Product product) //aizpildam offer pogas saturu
        {
            if (btn.Content is StackPanel stack)
            {
                if (stack.Children[0] is TextBlock qtyTxt) //atrodam pogu "konteinera" bērnus un aizpildam ar datiem
                    qtyTxt.Text = $"x{product.Quantity}";
                if (stack.Children[1] is Image img)
                    img.Source = new BitmapImage(new Uri(product.ImagePath, UriKind.RelativeOrAbsolute));
                if (stack.Children[2] is TextBlock priceTxt)
                    priceTxt.Text = $"{product.Price:F2}€";
            }
        }
        private void QuantityButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            string productName = lblProductName.Content as string;
            if (productName == null)
                return;

            // city offer
            Product cityOffer = cityOffers.FirstOrDefault(p => p.Name == productName);
            if (cityOffer == null)
                return;

            // inventory item
            Product invProduct = inventory.FirstOrDefault(p => p.Name == productName);

            int currentQty = int.TryParse(lblQuantity.Content.ToString(), out int qty) ? qty : 0;

            // ----- MODIFY QTY -----
            if ((button.Content as string) == "+")
            {
                lblQuantity.Content = (currentQty + 1).ToString();
            }
            else
            {
                if (currentQty > 0)
                    lblQuantity.Content = (currentQty - 1).ToString();
            }

            // recompute new quantity
            int newQty = int.Parse(lblQuantity.Content.ToString());

            // ---- SHOW CITY DEMAND INFO ----
            if (invProduct != null)  // only if item exists in inventory
            {
                int maxSell = cityOffer.MaxDemand;

                if (maxSell <= 0)
                {
                    // ONLY show this if user tries to increase beyond 0
                    if (newQty > 0)
                    {
                        _messageService.ShowMessage(
                            $"The city doesn't want any more {invProduct.Name}.",
                            MessageType.Warning
                        );
                    }
                }
                else
                {
                    // show how many can still be sold
                    if (newQty < maxSell)
                    {
                        txtMessage.Text =
                            $"You can sell up to {maxSell} units of {invProduct.Name} to the city.";
                    }
                    else if (newQty == maxSell)
                    {
                        txtMessage.Text =
                            $"You are at the max amount the city wants ({maxSell}).";
                    }
                    else // newQty > maxSell
                    {
                        _messageService.ShowMessage(
                            $"The city doesn't want more than {maxSell} units of {invProduct.Name}.",
                            MessageType.Warning
                        );

                        // optional: block adding more
                        lblQuantity.Content = maxSell.ToString();
                    }
                }
            }

            CountTotalCost();
            UpdateTotalSellPriceDisplay();
        }
        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                lblProductName.Content = product.Name;
                UpdateTotalSellPriceDisplay(); //šajā eventā metode nostrāda, ja daudzums ir jau ievadīts, bet spēlētājs maina produktu
                txtMessage.Text = "";
            }
        }
        private void GenerateCityOffer()
        {
            HashSet<string> usedNames = new HashSet<string>(); //lai nodrošinātu, ka nav divi vienādi nosaukumi
            while (cityOffers.Count < 6) //ģenerējam 6 dažādus produktus
            {
                Product p = new Product(true); //izsaucam konstruktoru ar true, lai uzģenerētu klasē definētās noklusējuma vērtības
                if (!usedNames.Contains(p.Name)) //pārbauda, vai nosaukums jau nav izmantots
                {
                    cityOffers.Add(p); //pievieno produktu piedāvājumam
                    usedNames.Add(p.Name); //pievieno nosaukumu izmantoto nosaukumu kopai
                }
            }
            for (int i = 0; i < OfferGrid.Children.Count && i < cityOffers.Count; i++) //aizpilda pogas OfferGrid ar uzģenerētajiem produktiem
            {
                if (OfferGrid.Children[i] is Button btn) //pārbauda, vai bērns ir poga
                {
                    Product product = cityOffers[i]; //iegūst produktu no piedāvājuma saraksta

                    StackPanel stack = (StackPanel)btn.Content; //pogas saturs ir StackPanel, tāpēc to pārvērš par StackPanel tipu
                    TextBlock txt2 = (TextBlock)stack.Children[0]; //atrodam StackPanel bērnus
                    Image img = (Image)stack.Children[1]; //atrodam Image bērnu
                    TextBlock txt = (TextBlock)stack.Children[2]; //atrodam TextBlock bērnu


                    img.SetBinding(Image.SourceProperty, new Binding("ImagePath")); //saista Image avotu ar produkta ImagePath īpašību
                    img.Source = new BitmapImage(new Uri(product.ImagePath, UriKind.RelativeOrAbsolute)); //iestata Image avotu

                    txt2.Text = $"x {product.Quantity.ToString()}"; //iestata daudzumu, kas arī nāk no uzģenerētā produkta
                    txt.Text = $"{product.Price:F2}€"; //iestata cenu, kas arī nāk no uzģenerētā produkta
                    btn.Tag = product; //iestata pogas Tag īpašību ar produktu, lai vēlāk varētu piekļūt šim produktam
                }
            }
        }
        private void CountTotalCost()
        {
            // aprēķina kopējo pirkšanas cenu, pamatojoties uz izvēlēto produktu un daudzumu
            if (lblProductName.Content is string productName && !string.IsNullOrEmpty(productName) && int.TryParse(lblQuantity.Content.ToString(), out int quantity)) //ja ir izvēlēts produkts un daudzums ir derīgs skaitlis
            {
                Product product = cityOffers.FirstOrDefault(p => p.Name == productName); //atrodam city offer piedāvājumā izvēlētā produkta objektu
                if (product != null)
                {
                    decimal totalCost = product.Price * quantity; //aprēķina kopējo cenu
                    lblBuyPrice.Content = $"{totalCost:F2}€"; //iestata kopējo cenu tekstā
                }
                else
                {
                    lblBuyPrice.Content = "0.00€";
                }
            }
            else
            {
                lblBuyPrice.Content = "0.00€";
            }
        }
        private decimal CalculateDiscountedSellPrice(decimal basePrice, Freshness freshness, int quantity) //aprēķina kopējo pārdošanas cenu, ņemot vērā svaigumu un daudzumu
        {
            if (quantity <= 0)
                return 0m;

            decimal discountedPrice = basePrice * quantity * 0.8m; // -20% visiem produktiem
            switch (freshness)
            {
                case Freshness.Normal:
                    discountedPrice *= 0.85m; // -15%
                    break;
                case Freshness.Expired:
                    discountedPrice *= 0.50m; // -50%
                    break;
                case Freshness.Rotten:
                    discountedPrice = 0m; // bezvērtīgs, bet arī nevar pārdot - iestatīts, lai citā metodē varētu izsekot
                    break;
            }
            return discountedPrice;
        }
        //private void ChangeConditionAndTick()
        //{
        //    // Use the class-level toRemove (clear it for this tick)
        //    toRemove.Clear();

        //    // Iterate over a snapshot so removing items from inventory is safe
        //    foreach (var invProduct in inventory.ToList())
        //    {
        //        // decrement ticks
        //        invProduct.TicksToExpire--;

        //        if (invProduct.TicksToExpire <= 0)
        //        {
        //            switch (invProduct.Freshness)
        //            {
        //                case Freshness.Fresh:
        //                    invProduct.Freshness = Freshness.Normal;
        //                    invProduct.TicksToExpire = random.Next(1, 4);
        //                    break;

        //                case Freshness.Normal:
        //                    invProduct.Freshness = Freshness.Expired;
        //                    invProduct.TicksToExpire = random.Next(1, 3);
        //                    break;

        //                case Freshness.Expired:
        //                    invProduct.Freshness = Freshness.Rotten;
        //                    // mark for removal after iteration
        //                    toRemove.Add(invProduct);
        //                    break;
        //            }

        //            if (invProduct.Freshness == Freshness.Rotten)
        //            {
        //                invProduct.TicksToExpire = 0;
        //                _messageService.ShowMessage($"Oh, no! Your {invProduct.Name} has been thrown away.", MessageType.Error, useTimer: true);
        //            }
        //            else if (invProduct.Freshness == Freshness.Expired)
        //            {
        //                _messageService.ShowMessage("You have some old stuff in your inventory.", MessageType.Warning, useTimer: true);
        //            }
        //        }
        //    }

        //    // Remove rotten items that were collected in the class-level list
        //    foreach (var item in toRemove.ToList())
        //    {
        //        inventory.Remove(item);
        //    }
        //    UpdateInventoryUI();
        //}
        private void ChangeConditionAndTick()
        {
            toRemove.Clear();
            var messagesThisTick = new List<(string text, MessageType type)>();

            foreach (var invProduct in inventory.ToList())
            {
                invProduct.TicksToExpire--;

                if (invProduct.TicksToExpire <= 0)
                {
                    switch (invProduct.Freshness)
                    {
                        case Freshness.Fresh:
                            invProduct.Freshness = Freshness.Normal;
                            invProduct.TicksToExpire = random.Next(1, 4);
                            break;

                        case Freshness.Normal:
                            invProduct.Freshness = Freshness.Expired;
                            invProduct.TicksToExpire = random.Next(1, 3);
                            messagesThisTick.Add(("You have some old stuff in your inventory.", MessageType.Warning));
                            break;

                        case Freshness.Expired:
                            invProduct.Freshness = Freshness.Rotten;
                            toRemove.Add(invProduct);
                            messagesThisTick.Add(($"Oh, no! Your {invProduct.Name} has been thrown away.", MessageType.Error));
                            break;
                    }
                }
            }

            // parādām ziņas pēc tam, kad iterācija pabeigta
            foreach (var msg in messagesThisTick)
                _messageService.ShowMessage(msg.text, msg.type);

            // izņemam rotten produktus
            foreach (var item in toRemove)
                inventory.Remove(item);

            UpdateInventoryUI();
        }
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (lblProductName.Content is string productName && !string.IsNullOrEmpty(productName) && int.TryParse(lblQuantity.Content.ToString(), out int quantityToBuy)) //ja ir izvēlēts produkts un daudzums ir derīgs skaitlis
            {
                Product selectedProduct = cityOffers.FirstOrDefault(p => p.Name == productName); //atrodam city offer piedāvājumā izvēlētā produkta objektu
                if (selectedProduct == null)
                {
                    _messageService.ShowMessage("Please select a product to buy.", MessageType.Error);
                    return;
                }
                if (quantityToBuy > selectedProduct.Quantity)
                {
                    _messageService.ShowMessage($"Cannot buy more than {selectedProduct.Quantity} units of {selectedProduct.Name}.", MessageType.Warning);
                    return;
                }
                if (quantityToBuy <= 0)
                {
                    _messageService.ShowMessage("Please enter a quantity you want to buy.", MessageType.Error);
                    return;
                }
                Product existing = inventory.FirstOrDefault(p => //p ir tāds pats produkts inventārā
                    p.Name == selectedProduct.Name &&
                    p.Freshness == selectedProduct.Freshness &&
                    p.TicksToExpire == selectedProduct.TicksToExpire);
                decimal totalCost = selectedProduct.Price * quantityToBuy; //aprēķina kopējo pirkšanas cenu
                if (totalCost > (decimal)account.GetBalance()) //pārbauda, vai spēlētājam ir pietiekami daudz naudas
                {
                    _messageService.ShowMessage("Not enough money to complete the purchase.", MessageType.Warning);
                    return;
                }
                else
                {
                    if (existing != null)
                    {
                        existing.Quantity += quantityToBuy; //ja tāds pats produkts ir inventārā, palielina tā daudzumu
                    }
                    else
                    {
                        Product newProduct = new Product(false) //ja tāds pats produkts nav inventārā, izveido jaunu produktu ar tādām pašām īpašībām kā izvēlētajam produktam
                        {
                            Name = selectedProduct.Name,
                            Quantity = quantityToBuy,
                            Freshness = selectedProduct.Freshness,
                            TicksToExpire = selectedProduct.TicksToExpire,
                            Type = selectedProduct.Type,
                        };
                        inventory.Add(newProduct); //pievieno jauno produktu inventāram
                    }
                    selectedProduct.Quantity -= quantityToBuy; //samazina izvēlētā produkta daudzumu piedāvājumā

                    UpdateCityOfferUI(selectedProduct);
                    lblQuantity.Content = "0";
                    lblBuyPrice.Content = "0.00€";
                    lblProductName.Content = "";
                    account.DecreaseBalance((double)totalCost);
                    UpdateBalance();
                    UpdateInventoryUI();
                    _messageService.ShowMessage($"You bought {quantityToBuy} units of {selectedProduct.Name}, and spent {totalCost}€.", MessageType.Info);
                }
            }
            else
            {
                _messageService.ShowMessage("Please select a valid product and quantity to buy.", MessageType.Error);
                return;
            }
        }
        private void UpdateInventoryUI()
        {
            for (int i = 0; i < productsInventory.Children.Count; i++) //iet cauri visiem Inventory Grid bērniem
            {
                if (productsInventory.Children[i] is StackPanel slotPanel) //pārbauda, vai bērns ir StackPanel (slots)
                {
                    // atrodam Image un TextBlock bērnus
                    var img = slotPanel.Children.OfType<Image>().FirstOrDefault(x => (string)x.Tag == "invImg");
                    var txt = slotPanel.Children.OfType<TextBlock>().FirstOrDefault(x => (string)x.Tag == "invTxt");

                    if (i < inventory.Count) //ja slots ir aizpildāms ar produktu no inventāra saraksta
                    {
                        Product product = inventory[i]; //iegūst produktu no inventāra saraksta

                        if (product != null)
                        {
                            // attēls un teksts
                            if (img != null)
                                img.Source = new BitmapImage(new Uri(product.ImagePath, UriKind.RelativeOrAbsolute));
                            if (txt != null)
                                txt.Text = $"{product.Name} x{product.Quantity}";

                            // fona gradienta uzstādīšana
                            UpdateInventorySlotBackground(slotPanel, product); //izsauc metodi, lai uzstādītu fona gradientu atbilstoši produkta svaigumam
                        }
                    }
                    else
                    {
                        // tukšais slotiņš (spēlētājs redz tukšumu)
                        if (img != null)
                            img.Source = null;
                        if (txt != null)
                            txt.Text = "";
                        slotPanel.Background = null;
                    }
                }
            }

        }
        private void UpdateCityOfferUI(Product updatedProduct)
        {   //atjauno konkrētā produkta UI piedāvājumā, lai atspoguļotu izmaiņas pēc pirkšanas
            if (updatedProduct != null)
            {
                for (int i = 0; i < OfferGrid.Children.Count; i++)
                {
                    if (OfferGrid.Children[i] is Button btn && btn.Tag is Product product && product.Name == updatedProduct.Name) //atrodam pogu, kuras Tag ir tas pats produkts, kas tika atjaunots
                    {
                        StackPanel stack = (StackPanel)btn.Content;
                        TextBlock txt2 = (TextBlock)stack.Children[0];
                        Image img = (Image)stack.Children[1];
                        TextBlock txt = (TextBlock)stack.Children[2];
                        img.SetBinding(Image.SourceProperty, new Binding("ImagePath"));
                        img.Source = new BitmapImage(new Uri(product.ImagePath, UriKind.RelativeOrAbsolute));
                        txt2.Text = $"x {product.Quantity.ToString()}";
                        txt.Text = $"{product.Price:F2}€";
                        btn.Tag = product;
                        btn.IsEnabled = product.Quantity > 0; // ja produkta daudzums ir 0, poga tiek atspējota

                        break;
                    }
                }
            }
        }
        private void UpdateInventorySlotBackground(StackPanel slotPanel, Product product)
        {
            {
                if (slotPanel == null || product == null)
                    return;

                Color centerColor;

                switch (product.Freshness)
                {
                    case Freshness.Fresh:
                        centerColor = Colors.LightGreen;
                        break;
                    case Freshness.Normal:
                        centerColor = Colors.Orange;
                        break;
                    case Freshness.Expired:
                        centerColor = Colors.Red;
                        break;
                    case Freshness.Rotten:
                        centerColor = Colors.Gray;
                        break;
                    default:
                        centerColor = Colors.LightGray;
                        break;
                }

                slotPanel.Background = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.5, 0.5),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.6,
                    RadiusY = 0.6,
                    GradientStops = new GradientStopCollection
                {
                    new GradientStop(centerColor, 0.0),
                    new GradientStop(Colors.Transparent, 1.0)
                }
                };
            }
        }
        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            if (lblProductName.Content is string productName && !string.IsNullOrEmpty(productName) && int.TryParse(lblQuantity.Content.ToString(), out int quantityToSell)) //ja ir izvēlēts produkts un daudzums ir derīgs skaitlis
            {
                Product invProduct = inventory
                .Where(p => p.Name == productName)
                .OrderBy(p => p.Freshness)
                .ThenBy(p => p.TicksToExpire)
                .FirstOrDefault();
                if (invProduct == null)
                {
                    _messageService.ShowMessage("Please select a product to sell.", MessageType.Error);
                    return;
                }
                if (quantityToSell > invProduct.Quantity)
                {
                    _messageService.ShowMessage($"Cannot sell more than {invProduct.Quantity} units of {invProduct.Name}.", MessageType.Warning);
                    return;
                }
                if (quantityToSell <= 0)
                {
                    _messageService.ShowMessage("Please enter a quantity you want to sell.", MessageType.Error);
                    return;
                }
                Product cityOffer = cityOffers.FirstOrDefault(p => p.Name == productName);
                if (cityOffer == null)
                {
                    _messageService.ShowMessage("No current city offer for this product.", MessageType.Error);
                    return;
                }
                int remainingToSell = cityOffer.MaxDemand;
                if (remainingToSell <= 0)
                {
                    _messageService.ShowMessage($"The city doesn't want any more {invProduct.Name}.", MessageType.Warning);
                    return;
                }
                int allowedToSell = Math.Min(quantityToSell, remainingToSell);
                if (allowedToSell < quantityToSell)
                {
                    _messageService.ShowMessage($"The city only accepted {allowedToSell} units of {invProduct.Name}. " +
                                    $"{quantityToSell - allowedToSell} remain in your inventory.", MessageType.Warning);
                }
                invProduct.Quantity -= allowedToSell;
                cityOffer.MaxDemand -= allowedToSell;
                decimal totalRevenue = CalculateDiscountedSellPrice(cityOffer.Price, invProduct.Freshness, allowedToSell);
                account.IncreaseBalance((double)totalRevenue);
                lblSellPrice.Content = $"{totalRevenue:F2}€";

                if (invProduct.Quantity <= 0)
                {
                    inventory.Remove(invProduct);
                }

                UpdateBalance();
                lblQuantity.Content = "0";
                UpdateInventoryUI();
                CheckPlayerBalance();
                GetNewBestScore();
                lblBuyPrice.Content = "0.00€";
                lblSellPrice.Content = "0.00€";
                lblProductName.Content = "";
                txtMessage.Text = "";
                _messageService.ShowMessage($"You sold {allowedToSell} units of {invProduct.Name}, and earned {totalRevenue:F2}€.", MessageType.Info);
            }
            else
            {
                _messageService.ShowMessage("Please select a valid product and quantity to sell.", MessageType.Error);
            }
        }
        private void UpdateTotalSellPriceDisplay()
        {
            if (lblProductName.Content is string productName && !string.IsNullOrEmpty(productName) && int.TryParse(lblQuantity.Content.ToString(), out int quantityToSell) && quantityToSell > 0)
            {
                Product invProduct = inventory.FirstOrDefault(p => p.Name == productName);
                Product cityOffer = cityOffers.FirstOrDefault(p => p.Name == productName);
                if (invProduct != null && cityOffer != null)
                {
                    decimal discountedPrice = CalculateDiscountedSellPrice(cityOffer.Price, invProduct.Freshness, quantityToSell);
                    lblSellPrice.Content = $"{discountedPrice:F2}€";
                    return;
                }
            }
            lblSellPrice.Content = "0.00€";
        }
        private void CheckPlayerBalance()
        {
            bool hasProducts = inventory.Count > 0 && inventory.Any(item => item.Quantity > 0);
            double playerBalance = account.GetBalance();

            if (playerBalance < 10 && !hasProducts)
            {
                GameOverTriggered?.Invoke();
                GameOverOverlay.Visibility = Visibility.Visible;
            }
        }
        private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            BackToMainMenuRequested?.Invoke();
            if (GameOverOverlay != null)
                GameOverOverlay.Visibility = Visibility.Collapsed;
        }
        public void SaveGameStateToJson()
        {
            if (string.IsNullOrEmpty(CurrentSaveFilePath))
                throw new InvalidOperationException("Save file path is not set.");

            GameState gameState = new GameState
            {
                PlayerBalance = account.GetBalance(),
                Inventory = inventory,
                CityOffers = cityOffers,
                ToRemove = toRemove,
                CurrentCity = CityLabel.Content.ToString(),
                BestScore = BestScoreBlock.Text.Contains(":") ? double.Parse(BestScoreBlock.Text.Split(':')[1].Trim().Replace("€", "")) : 0
            };
            File.WriteAllText(CurrentSaveFilePath, JsonConvert.SerializeObject(gameState, Formatting.Indented));
            _messageService.ShowMessage("Game saved successfully.", MessageType.Info);
        }
        private void UpdateUI()
        {
            account.DecreaseBalance(account.GetBalance());
            account.IncreaseBalance(CurrentState.PlayerBalance);

            inventory = CurrentState.Inventory;
            cityOffers = CurrentState.CityOffers;
            toRemove = CurrentState.ToRemove;

            CityLabel.Content = CurrentState.CurrentCity;
            BestScoreBlock.Text = $"Best Score: {CurrentState.BestScore:F2}€";

            UpdateBalance();
            UpdateInventoryUI();
            GenerateCityOffer();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveGameStateToJson();
            }
            catch (Exception ex)
            {
                _messageService.ShowMessage($"Error saving game: {ex.Message}", MessageType.Error);
            }
        }
        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            SavedGamesRequested?.Invoke();
            if (GameOverOverlay != null)
                GameOverOverlay.Visibility = Visibility.Collapsed;

        }
    }
}
