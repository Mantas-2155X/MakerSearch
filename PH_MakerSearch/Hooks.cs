using System;
using System.Timers;
using System.Collections.Generic;

using BepInEx;
using HarmonyLib;

using UniRx;
using UniRx.Triggers;

namespace PH_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EditMode), "Setup")]
        private static void EditMode_Setup_CreateUI(EditMode __instance, MoveableThumbnailSelectUI ___itemSelectUI)
        {
            Tools.CreateUI(__instance, ___itemSelectUI);
            SetupCache(__instance);
        }
        
        private static Timer _cacheSaveTimer;

        private static void SetupCache(EditMode __instance)
        {
            Cacher.ReadCache();

            void OnSave(object sender, ElapsedEventArgs args)
            {
                _cacheSaveTimer.Stop();
                Cacher.WriteCache();
            }

            // Timeout has to be long enough to ensure people with potato internet can still get the translations in time
            _cacheSaveTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            _cacheSaveTimer.Elapsed += OnSave;
            _cacheSaveTimer.AutoReset = false;
            _cacheSaveTimer.SynchronizingObject = ThreadingHelper.SynchronizingObject;

            // If a cache save is still pending on maker exit, run it immediately
            __instance.OnDestroyAsObservable().Subscribe(_ =>
            {
                if (_cacheSaveTimer.Enabled) OnSave(null, null);
            });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ThumbnailSelectUI), "SetupCells")]
        private static void CustomSelectListCtrl_SetupCells_GetSelectInfos(List<CustomSelectSet> ___datas)
        {
            if (___datas == null)
                return;

            var anyTranslations = false;

            for (var i = 0; i < ___datas.Count; i++)
            {
                if(Tools.searchNameStrings.ContainsKey(___datas[i].name))
                    continue;
                
                if (Cacher.TranslationLookup.TryGetValue(___datas[i].name, out var tl))
                {
                    Tools.searchNameStrings[___datas[i].name] = ___datas[i].name + "/v" + tl;
                    continue;
                }

                var idx = i;
                TranslationHelper.Translate(___datas[i].name, s =>
                {
                    Tools.searchNameStrings[___datas[idx].name] = ___datas[idx].name + "/v" + s;
                    Cacher.TranslationLookup[___datas[idx].name] = s;
                });

                anyTranslations = true;
            }

            if (anyTranslations)
            {
                // Reset the timer so it's counting since the last translation
                _cacheSaveTimer.Stop();
                _cacheSaveTimer.Start();
            }
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(MoveableThumbnailSelectUI), "UpdateEnables")]
        private static void MoveableThumbnailSelectUI_UpdateEnables_ResetSearch(ThumbnailSelectUI ___select)
        {
            if (___select == PH_MakerSearch.selectUI)
                Tools.ResetSearch();
        }
    }
}