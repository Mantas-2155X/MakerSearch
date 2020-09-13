using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using MessagePack;

namespace PH_MakerSearch
{
    public static class Cacher
    {
        private static List<TranslationCacheEntry> Cache = new List<TranslationCacheEntry>();
        
        public static List<TranslationCacheEntry> ReadCache()
        {
            if (!File.Exists(PH_MakerSearch.TranslationCachePath))
                return Cache ?? (Cache = new List<TranslationCacheEntry>());
            
            try
            {
                var cacheBytes = File.ReadAllBytes(PH_MakerSearch.TranslationCachePath);
                Cache = MessagePackSerializer.Deserialize<List<TranslationCacheEntry>>(cacheBytes);
            }
            catch (Exception e)
            {
                Console.Write("Failed reading MakerSearch cache: " + e);
            }

            return Cache ?? (Cache = new List<TranslationCacheEntry>());
        }

        public static void WriteCache()
        {
            try
            {
                var newCache = new List<TranslationCacheEntry>();

                foreach (var pair in Tools.searchNameStrings)
                {
                    var entry = new TranslationCacheEntry {OriginalName = pair.Key.name, TranslatedName = pair.Value};
                    
                    if(newCache.Contains(entry))
                        continue;

                    newCache.Add(entry);
                }

                Cache = newCache;
                
                var data = MessagePackSerializer.Serialize(Cache);
                File.WriteAllBytes(PH_MakerSearch.TranslationCachePath, data);
            }
            catch(Exception e)
            {
                Console.Write("Failed writing MakerSearch cache: " + e);
            }
        }

        public static Dictionary<string, string> CacheToDict(List<TranslationCacheEntry> cache)
        {
            var dict = new Dictionary<string, string>();

            if (cache == null)
                return dict;
            
            foreach (var entry in cache.Where(entry => !dict.ContainsKey(entry.OriginalName)))
                dict.Add(entry.OriginalName, entry.TranslatedName);

            return dict;
        }
    }
    
    [MessagePackObject]
    public sealed class TranslationCacheEntry
    {
        [Key(0)]
        public string OriginalName;
        [Key(1)]
        public string TranslatedName;
    }
}