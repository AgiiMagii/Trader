using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class BestScoreService
    {
        public void UpdateBestScore(string gameName, double newScore)
        {
            string saveDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameData");
            Directory.CreateDirectory(saveDir);
            string bestScoreFilePath = Path.Combine(saveDir, "best_scores.json");

            BestScoreInfo bestScores;

            if (File.Exists(bestScoreFilePath))
            {
                string json = File.ReadAllText(bestScoreFilePath);
                bestScores = Newtonsoft.Json.JsonConvert.DeserializeObject<BestScoreInfo>(json);
            }
            else
            {
                bestScores = new BestScoreInfo
                {
                    GameName = gameName,
                    BestScore = 0
                };
            }
            if (newScore > bestScores.BestScore)
            {
                bestScores.BestScore = newScore;
                bestScores.GameName = gameName;
                string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(bestScores, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(bestScoreFilePath, updatedJson);
            }
        }
    }
}
