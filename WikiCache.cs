using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MySql.Data.MySqlClient;
using RYCBEditorX.MySQL;
using RYCBEditorX.Crossing;

namespace RYCBEditorX.Utils
{
    public static class WikiCache
    {
        private static readonly string CacheDir = Path.Combine(GlobalConfig.StartupPath, "Cache", "online-cache");
        private static readonly string WikiCacheFile = Path.Combine(CacheDir, "wiki_cache.json");

        public static void EnsureCacheDirectory()
        {
            if (!Directory.Exists(CacheDir))
            {
                Directory.CreateDirectory(CacheDir);
            }
        }

        public static void SaveToCache(Dictionary<string, List<string>> wikiData)
        {
            EnsureCacheDirectory();
            var json = JsonSerializer.Serialize(wikiData);
            File.WriteAllText(WikiCacheFile, json);
        }

        public static Dictionary<string, List<string>> LoadFromCache()
        {
            EnsureCacheDirectory();
            if (File.Exists(WikiCacheFile))
            {
                try
                {
                    var json = File.ReadAllText(WikiCacheFile);
                    return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                }
                catch
                {
                    return new Dictionary<string, List<string>>();
                }
            }
            return new Dictionary<string, List<string>>();
        }
    }
}