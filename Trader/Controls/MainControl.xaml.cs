﻿using System;
using System.Collections.Generic;
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
using Trader.Lib;
using Trader.Controls; // Add this line if SavedGamesOverlay is in the Trader.Controls namespace
using System.IO; // Add this using directive at the top with the others

namespace Trader.Controls
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }
        public void NewGame_Click(object sender, RoutedEventArgs e)
        {
            LoadGameControlWithState(new GameState());
        }
        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        public void SavedGames_Click(object sender, RoutedEventArgs e)
        {
            LoadGamesList();
            mainButtonGrid.Visibility = Visibility.Collapsed;
            SavedGamesOverlay.Visibility = Visibility.Visible;
        }
        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            if (SavedGamesList.SelectedItem == null)
            {
                MessageBox.Show("Please select a saved game to load.");
                return;
            }
            string selectedSave = SavedGamesList.SelectedItem.ToString();
            string savePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", selectedSave + ".json");

            Json json = new Json();
            GameState state = null;
            try
            {
                state = json.CreateObject<GameState>(savePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load saved game: " + ex.Message);
                return;
            }
            if (state != null)
            {
                LoadGameControlWithState(state, savePath);
                SavedGamesOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Failed to load saved game.");
            }
        }
        private void LoadGameControlWithState(GameState state, string savePath=null)
        {
            Window mainWindow = Application.Current.MainWindow;
            GameControl gameControl = new GameControl()
            {
                CurrentSaveFilePath = savePath
            };
            gameControl.LoadState(state);
            gameControl.BackToMainMenuRequested += () =>
            {
                mainWindow.Content = this;
                mainButtonGrid.Visibility = Visibility.Visible;
                SavedGamesOverlay.Visibility = Visibility.Collapsed;
            };
            mainWindow.Content = gameControl;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SavedGamesOverlay.Visibility = Visibility.Collapsed;
            mainButtonGrid.Visibility = Visibility.Visible;
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SavedGamesList.SelectedItem == null)
            {
                MessageBox.Show("Please select a saved game to delete.");
                return;
            }
            string selectedSave = SavedGamesList.SelectedItem.ToString();
            string savePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves", selectedSave + ".json");
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    MessageBox.Show("Saved game deleted.");
                    LoadGamesList();
                }
                else
                {
                    MessageBox.Show("Selected saved game file does not exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete saved game: " + ex.Message);
            }
        }
        private void LoadGamesList()
        {
            string savesDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            try
            {
                List<string> saves = System.IO.Directory.GetFiles(savesDirectory, "*.json")
                                           .Select(System.IO.Path.GetFileNameWithoutExtension)
                                           .ToList();
                SavedGamesList.ItemsSource = saves;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to access or create saves directory: " + ex.Message);
                return;
            }
        }
    }
}
