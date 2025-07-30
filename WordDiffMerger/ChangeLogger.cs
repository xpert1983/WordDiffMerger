using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace WordDiffMerger
{
    public static class ChangeLogger
    {
        public static void SaveLog(List<ChangeSet> allChanges, string logPath)
        {
            // ЯВНО указываем Formatting через JsonConvert, чтобы не было конфликта имён
            var json = JsonConvert.SerializeObject(allChanges, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(logPath, json, new UTF8Encoding(true)); // UTF-8 BOM
        }
    }
}