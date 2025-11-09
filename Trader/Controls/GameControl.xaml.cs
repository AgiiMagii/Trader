using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Trader.Lib;
using static System.Windows.Forms.AxHost;
using static Trader.Lib.Enums;

namespace Trader.Controls
{
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public event Action GameOverTriggered;
        public event Action BackToMainMenuRequested;
        public string CurrentSaveFilePath { get; set; }
        Account account = new Account();
        Random random = new Random();
        private List<Product> inventory = new List<Product>();
        private List<Product> cityOffers = new List<Product>();
        private List<Product> toRemove = new List<Product>();
        private GameState CurrentState;
        private DispatcherTimer messageTimer;
        public GameControl(GameState initialState = null)
        {
            InitializeComponent();
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
        private void CityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                cityOffers.Clear();
                CityLabel.Content = button.Content.ToString();
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
                foreach (var product in inventory.ToList())
                {
                    ChangeConditionAndTick();
                }
            }
            lblMessage.Content = "";
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
            if (sender is Button button)
            {
                int currentQty = int.TryParse(lblQuantity.Content.ToString(), out int qty) ? qty : 0; //nolasa pašreizējo daudzumu daudzuma lauciņā

                if (lblProductName.Content is string productName) //nolasa izvēlēto produktu, jauns produkts ir izvēlēts
                {
                    Product cityOffer = cityOffers.FirstOrDefault(p => p.Name == productName); //atrodam city offer piedāvājumā izvēlētā produkta objektu

                    if (cityOffer != null) //ja produkts ir piedāvājumā, tad palielina vai samazina daudzumu
                    {
                        if ((button.Content as string) == "+") //ja nospiesta plus poga
                        {
                            lblQuantity.Content = (currentQty + 1).ToString(); //palielina daudzumu par 1

                            // Tiklīdz izvēlētais produkts ir arī inventārā, rādam info par city demand
                            Product invProduct = inventory.FirstOrDefault(p => p.Name == productName); //atrodam inventārā izvēlētā produkta objektu
                            if (invProduct != null) //ja produkts ir inventārā, tad rādam info par city demand (cik var pārdot pilsētai)
                            {
                                int remainingToSell = cityOffer.MaxDemand; //pilsētas pieprasījums pēc šī produkta (randoma vērtība no 1-10)
                                if (remainingToSell <= 0) //ja pieprasījums ir 0 vai mazāks, tad pilsēta vairs nevēlas šo produktu
                                {
                                    lblMessage.Content = $"The city doesn't want any more {invProduct.Name}.";
                                }
                                else
                                {
                                    lblMessage.Content = $"City will accept up to {remainingToSell} units of {invProduct.Name}.";
                                }
                            }
                            else
                            {
                                // Citi paziņojumi pirkšanai, ja nepieciešams
                                lblMessage.Content = "";
                            }
                        }
                        else
                        {
                            if (currentQty > 0) //ja nospiesta mīnus poga un daudzums ir lielāks par 0
                                lblQuantity.Content = (currentQty - 1).ToString(); //samazina daudzumu par 1
                        }
                    }
                }
            }
            CountTotalCost(); //pārskaita kopējo pirkšanas cenu
            UpdateTotalSellPriceDisplay(); //atjauno kopējo pārdošanas cenu, ja tāds pats produkts ir inventārā
        }
        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                lblProductName.Content = product.Name; //uzstāda izvēlēto produkta nosaukumu
                UpdateTotalSellPriceDisplay(); //šajā eventā metode nostrāda, ja daudzums ir jau ievadīts, bet spēlētājs maina produktu
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
                var product = cityOffers.FirstOrDefault(p => p.Name == productName); //atrodam city offer piedāvājumā izvēlētā produkta objektu
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
        private void ChangeConditionAndTick()
        {
            foreach (var invProduct in inventory) //iet cauri visiem inventāra produktiem
            {
                invProduct.TicksToExpire--; //samazina atlikušos "gājienus" līdz produkta stāvokļa maiņai
                                            //ja atlikušais gājienu skaits ir 0 vai mazāks, maina produkta stāvokli uz nākamo

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
                            break;

                        case Freshness.Expired:
                            invProduct.Freshness = Freshness.Rotten;
                            toRemove.Add(invProduct);
                            break;

                        case Freshness.Rotten:
                            break;
                    }
                }
            }
            foreach (var item in toRemove)
            {
                inventory.Remove(item); // ja produkts ir sapuvusi, izņem to no inventāra
            }
            UpdateInventoryUI(); //atjauno inventāra UI, lai atspoguļotu izmaiņas
        }
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (lblProductName.Content is string productName && !string.IsNullOrEmpty(productName) && int.TryParse(lblQuantity.Content.ToString(), out int quantityToBuy)) //ja ir izvēlēts produkts un daudzums ir derīgs skaitlis
            {
                Product selectedProduct = cityOffers.FirstOrDefault(p => p.Name == productName); //atrodam city offer piedāvājumā izvēlētā produkta objektu
                if (selectedProduct == null)
                {
                    MessageBox.Show("Please select a product to buy.");
                    return;
                }
                if (quantityToBuy > selectedProduct.Quantity)
                {
                    MessageBox.Show($"Cannot buy more than {selectedProduct.Quantity} units of {selectedProduct.Name}.");
                    return;
                }
                if (quantityToBuy <= 0)
                {
                    MessageBox.Show("Please enter a quantity you want to buy.");
                    return;
                }
                Product existing = inventory.FirstOrDefault(p => //p ir tāds pats produkts inventārā
                    p.Name == selectedProduct.Name &&
                    p.Freshness == selectedProduct.Freshness &&
                    p.TicksToExpire == selectedProduct.TicksToExpire);
                decimal totalCost = selectedProduct.Price * quantityToBuy; //aprēķina kopējo pirkšanas cenu
                if (totalCost > (decimal)account.GetBalance()) //pārbauda, vai spēlētājam ir pietiekami daudz naudas
                {
                    MessageBox.Show("Not enough balance to complete the purchase.");
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
                        var newProduct = new Product(false) //ja tāds pats produkts nav inventārā, izveido jaunu produktu ar tādām pašām īpašībām kā izvēlētajam produktam
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

                    UpdateCityOfferUI(selectedProduct); //atjauno konkretā produkta UI piedāvājumā, lai atspoguļotu izmaiņas
                    lblQuantity.Content = "0";
                    lblBuyPrice.Content = "0.00€";
                    lblProductName.Content = "";
                    account.DecreaseBalance((double)totalCost); //samazina spēlētāja bilanci par kopējo pirkšanas cenu
                    UpdateBalance(); //atjauno bilances UI
                    UpdateInventoryUI(); //atjauno inventāra UI, lai atspoguļotu izmaiņas
                }
            }
            else
            {
                MessageBox.Show("Please select a valid product and quantity to buy.");
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
                    MessageBox.Show("Please select a product to sell from your inventory.");
                    return;
                }
                if (quantityToSell > invProduct.Quantity)
                {
                    MessageBox.Show($"Cannot sell more than {invProduct.Quantity} units of {invProduct.Name}.");
                    return;
                }
                if (quantityToSell <= 0)
                {
                    MessageBox.Show("Please enter a quantity you want to sell.");
                    return;
                }
                Product cityOffer = cityOffers.FirstOrDefault(p => p.Name == productName);
                if (cityOffer == null)
                {
                    MessageBox.Show("No current city offer for this product.");
                    return;
                }
                int remainingToSell = cityOffer.MaxDemand;
                if (remainingToSell <= 0)
                {
                    MessageBox.Show($"The city doesn't want any more {invProduct.Name}.");
                    return;
                }
                int allowedToSell = Math.Min(quantityToSell, remainingToSell);
                if (allowedToSell < quantityToSell)
                {
                    MessageBox.Show($"The city only accepted {allowedToSell} units of {invProduct.Name}. " +
                                    $"{quantityToSell - allowedToSell} remain in your inventory.");
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
                lblBuyPrice.Content = "0.00€";
                lblSellPrice.Content = "0.00€";
                lblMessage.Content = "";
                lblProductName.Content = "";
            }
            else
            {
                MessageBox.Show("Please select a valid product and quantity to sell.");
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
                CurrentCity = CityLabel.Content.ToString()
            };
            File.WriteAllText(CurrentSaveFilePath, JsonConvert.SerializeObject(gameState, Formatting.Indented));
            ShowMessage("Game saved successfully.");
        }
        private void UpdateUI()
        {
            account.DecreaseBalance(account.GetBalance());
            account.IncreaseBalance(CurrentState.PlayerBalance);

            inventory = CurrentState.Inventory;
            cityOffers = CurrentState.CityOffers;
            toRemove = CurrentState.ToRemove;

            CityLabel.Content = CurrentState.CurrentCity;

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
                MessageBox.Show($"Error saving game: {ex.Message}");
            }
        }
        private void ShowMessage(string message)
        {
            lblMessage.Content = message;

            if (messageTimer != null)
                messageTimer.Stop();

            messageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            messageTimer.Tick += (s, e) =>
            {
                lblMessage.Content = string.Empty;
                messageTimer.Stop();
            };
            messageTimer.Start();
        }
    }
}
