using System;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using BepInEx;
using ChaCustom;
using MessagePack;
using UniRx;
using UniRx.Triggers;

namespace EC_MakerSearch
{
    public static class Cacher
    {
        public static Timer _cacheSaveTimer;
        public static Dictionary<string, string> TranslationLookup { get; private set; } = new Dictionary<string, string>();

        private static void ReadCache()
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

        private static void WriteCache()
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
        
        public static void SetupCache()
        {
            ReadCache();

            void OnSave(object sender, ElapsedEventArgs args)
            {
                _cacheSaveTimer.Stop();
                WriteCache();
            }

            // Timeout has to be long enough to ensure people with potato internet can still get the translations in time
            _cacheSaveTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            _cacheSaveTimer.Elapsed += OnSave;
            _cacheSaveTimer.AutoReset = false;
            _cacheSaveTimer.SynchronizingObject = ThreadingHelper.SynchronizingObject;

            // If a cache save is still pending on maker exit, run it immediately
            CustomBase.Instance.OnDestroyAsObservable().Subscribe(_ => { if (_cacheSaveTimer.Enabled) OnSave(null, null); });
        }
    }
}