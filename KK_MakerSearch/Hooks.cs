using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using ChaCustom;

namespace KK_MakerSearch
{
    public static class Hooks
    {
        private static readonly string[] ignoredDisvisibleCallers =
        {
            "MakerSearch_ResetDisables",
            "MakerSearch_Search"
        };

        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_CreateUI()
        {
            KK_MakerSearch.ctrl = null;

            Tools.disvisibleMemory.Clear();
            Tools.CreateUI();

            Cacher.SetupCache();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "DisvisibleItem", typeof(string), typeof(bool))]
        private static void CustomSelectListCtrl_DisvisibleItem_string_ReloadMemory(string name, bool disvisible, List<CustomSelectInfo> ___lstSelectInfo)
        {
            if (ignoredDisvisibleCallers.Contains(new StackFrame(2).GetMethod().Name))
                return;

            var customSelectInfo = ___lstSelectInfo.Find(item => item.name == name);
            if (customSelectInfo == null) 
                return;

            if (disvisible)
                Tools.disvisibleMemory.Add(customSelectInfo);
            else if (Tools.disvisibleMemory.Contains(customSelectInfo))
                Tools.disvisibleMemory.Remove(customSelectInfo);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "DisvisibleItem", typeof(int), typeof(bool))]
        private static void CustomSelectListCtrl_DisvisibleItem_int_ReloadMemory(int index, bool disvisible, List<CustomSelectInfo> ___lstSelectInfo)
        {
            if (ignoredDisvisibleCallers.Contains(new StackFrame(2).GetMethod().Name))
                return;

            var customSelectInfo = ___lstSelectInfo.Find(item => item.index == index);
            if (customSelectInfo == null) 
                return;

            if (disvisible)
                Tools.disvisibleMemory.Add(customSelectInfo);
            else if (Tools.disvisibleMemory.Contains(customSelectInfo))
                Tools.disvisibleMemory.Remove(customSelectInfo);
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
                
                var currentInfo = info;
                TranslationHelper.Translate(info.name, s =>
                {
                    Tools.searchNameStrings[currentInfo] = currentInfo.name + "/v" + s;
                    Cacher.TranslationLookup[currentInfo.name] = s;
                });

                anyTranslations = true;
            }

            if (anyTranslations)
            {
                // Reset the timer so it's counting since the last translation
                Cacher._cacheSaveTimer.Stop();
                Cacher._cacheSaveTimer.Start();
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