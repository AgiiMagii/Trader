using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Trader.Lib;
using System.IO;

namespace Trader.Controls
{
    public partial class MainControl : UserControl
    {
        public string CurrentSaveFilePath { get; set; }
        public MainControl()
        {
            InitializeComponent();
            LoadBestScore();
        }
        private void On_VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                LoadBestScore();
            }
        } // Refresh best score on visibility change to keep it up-to-date
        public void LoadBestScore()
        {
            if (!File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameData", "best_scores.json")))
            {
                return;
            }
            string bestScoreFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameData", "best_scores.json");
            string json = File.ReadAllText(bestScoreFilePath);

            BestScoreInfo bestScoreInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<BestScoreInfo>(json);
            if (bestScoreInfo != null)
            {
                BestScoreTextBlock.Text = $"{bestScoreInfo.BestScore.ToString("F2")} €";
                BestPlayerTextBlock.Text = $"(Game: {bestScoreInfo.GameName})";
            }
        } // Loads best score info from json which is kept up-to-date with other logic
        public void NewGame_Click(object sender, RoutedEventArgs e)
        {
            mainButtonGrid.Visibility = Visibility.Collapsed;
            NewGameOverlay.Visibility = Visibility.Visible;
        } // Opens New game overlay with its menu 
        private string GenerateNewSaveFilePath(string gameName)
        {
            string saveDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            Directory.CreateDirectory(saveDir);

            string filePath;
            int counter = 1;
            filePath = System.IO.Path.Combine(saveDir, $"{gameName}.json");

            while (File.Exists(filePath))
            {
                filePath = System.IO.Path.Combine(saveDir, $"Game {gameName} ({counter}).json");
                counter++;
            }

            return filePath;
        } // Generates unique save file path for new game with given name or default pattern
        private void PlayGame_Click(object sender, RoutedEventArgs e)
        {
            string gameName = txtGameName.Text.Trim();

            if (string.IsNullOrEmpty(gameName))
            {
                gameName = "";
            }

            if (gameName.Length > 20)
            {
                MessageBox.Show("Game name is too long. Max 20 characters.");
                return;
            }

            NewGameOverlay.Visibility = Visibility.Collapsed;

            CurrentSaveFilePath = GenerateNewSaveFilePath(gameName);

            LoadGameControlWithState(new GameState(), CurrentSaveFilePath);
        } // Opens Game control with new game state
        public void SavedGames_Click(object sender, RoutedEventArgs e)
        {
            LoadGamesList();
            mainButtonGrid.Visibility = Visibility.Collapsed;
            SavedGamesOverlay.Visibility = Visibility.Visible;
        } // Opens Saved games overlay with menu
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
        } // Loads saved games list from directory
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
            }
            else
            {
                MessageBox.Show("Failed to load saved game.");
            }
        } // Sets up Game control with loaded game state from selected save and calls it
        private void LoadGameControlWithState(GameState state, string savePath)
        {
            Window mainWindow = Application.Current.MainWindow;

            GameControl gameControl = new GameControl(state)
            {
                CurrentSaveFilePath = savePath
            };

            gameControl.BackToMainMenuRequested += () =>
            {
                mainWindow.Content = this;
                mainButtonGrid.Visibility = Visibility.Visible;
                SavedGamesOverlay.Visibility = Visibility.Collapsed;
            };
            gameControl.SavedGamesRequested += () =>
            {
                LoadGamesList();
                mainWindow.Content = this;
                mainButtonGrid.Visibility = Visibility.Collapsed;
                SavedGamesOverlay.Visibility = Visibility.Visible;
            };

            mainWindow.Content = gameControl;
        } // Helper to switch to GameControl with given state and save path. It also sets up the events for going back to main menu or saved games menu
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SavedGamesOverlay.Visibility = Visibility.Collapsed;
            mainButtonGrid.Visibility = Visibility.Visible;
        } // Closes the Saved games overlay if player wants to go back to main
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
        } // Deletes selected game from directory
        private void btnCloseOverlay_Click(object sender, RoutedEventArgs e)
        {
            NewGameOverlay.Visibility = Visibility.Collapsed;
            mainButtonGrid.Visibility = Visibility.Visible;
        } // Closes the New game overlay if player wants to go back to main
        public void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        } // Exit application
    }
}
