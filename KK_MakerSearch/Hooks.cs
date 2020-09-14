using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

using BepInEx;
using HarmonyLib;

using ChaCustom;
using UniRx;
using UniRx.Triggers;

namespace KK_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_CreateUI()
        {
            KK_MakerSearch.ctrl = null;

            Tools.disvisibleMemory.Clear();
            Tools.CreateUI();

            SetupCache();
        }

        private static Timer _cacheSaveTimer;
        private static void SetupCache()
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
            CustomBase.Instance.OnDestroyAsObservable().Subscribe(_ => { if (_cacheSaveTimer.Enabled) OnSave(null, null); });
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "Create")]
        private static void CustomSelectListCtrl_Create_GetSelectInfos(CustomSelectListCtrl __instance, List<CustomSelectInfo> ___lstSelectInfo)
        {
            if (___lstSelectInfo == null)
                return;

            var anyTranslations = false;
            foreach (var info in ___lstSelectInfo.Where(info => !Tools.searchNameStrings.ContainsKey(info)))
            {
                if (Cacher.TranslationLookup.TryGetValue(info.name, out var tl))
                {
                    Tools.searchNameStrings[info] = info.name + "/v" + tl;
                    continue;
                }

                TranslationHelper.Translate(info.name, s =>
                {
                    Tools.searchNameStrings[info] = info.name + "/v" + s;
                    Cacher.TranslationLookup[info.name] = s;
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

        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "Update")]
        private static void CustomSelectListCtrl_Update_ChangeController(CustomSelectListCtrl __instance)
        {
            if (KK_MakerSearch.ctrl == __instance)
                return;

            if (__instance.canvasGrp == null)
                return;

            if (!__instance.canvasGrp[0].name.Contains("win") || !__instance.canvasGrp[0].interactable || !__instance.canvasGrp[1].interactable || !__instance.canvasGrp[2].interactable)
                return;

            Tools.ResetSearch();

            KK_MakerSearch.ctrl = __instance;
            Tools.RememberDisvisibles();
        }
    }
}