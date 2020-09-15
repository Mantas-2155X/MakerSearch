using System;
using System.IO;
using System.Collections.Generic;

using MessagePack;

namespace EC_MakerSearch
{
    public static class Cacher
    {
        public static Dictionary<string, string> TranslationLookup { get; private set; } = new Dictionary<string, string>();

        public static void ReadCache()
        {
            if (!File.Exists(EC_MakerSearch.TranslationCachePath))
                return;

            try
            {
                UnityEngine.Debug.Log("Loading MakerSearch cache from " + EC_MakerSearch.TranslationCachePath);

                var cacheBytes = File.ReadAllBytes(EC_MakerSearch.TranslationCachePath);
                TranslationLookup = MessagePackSerializer.Deserialize<Dictionary<string, string>>(cacheBytes);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Failed reading MakerSearch cache: " + e);
            }
        }

        public static void WriteCache()
        {
            try
            {
                UnityEngine.Debug.Log("Writing MakerSearch cache to " + EC_MakerSearch.TranslationCachePath);

                var data = MessagePackSerializer.Serialize(TranslationLookup);
                File.WriteAllBytes(EC_MakerSearch.TranslationCachePath, data);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Failed writing MakerSearch cache: " + e);
            }
        }
    }
}