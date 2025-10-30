using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Lib
{
    public class Json
    {
        public bool CreateJson<Entity>(Entity entity, string saveDir) where Entity : class
        {
            try
            {
                string jsonContent = JsonConvert.SerializeObject(entity, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(saveDir, jsonContent);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public T CreateObject<T>(string filePath) where T : class
        {
            try
            {
                string content = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return null;
            }

        }

    }
}



