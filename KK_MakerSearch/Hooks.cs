using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HarmonyLib;

using ChaCustom;

namespace KK_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_CreateUI()
        {
            KK_MakerSearch.ctrl = null;
            
            Tools.disvisibleMemory.Clear();
            Tools.searchNameStrings.Clear();
            Tools.CreateUI();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "Create")]
        private static void CustomSelectListCtrl_Create_GetSelectInfos(List<CustomSelectInfo> ___lstSelectInfo)
        {
            if (___lstSelectInfo == null)
                return;

            var t = new Thread(TranslateItems)
            {
                IsBackground = true,
                Name = "Translate items",
                Priority = ThreadPriority.BelowNormal
            };
            
            t.Start();

            void TranslateItems()
            {
                foreach (var info in ___lstSelectInfo.Where(info => !Tools.searchNameStrings.ContainsKey(info)))
                    TranslationHelper.Translate(info.name, s => Tools.searchNameStrings[info] = info.name + "/v" + s);
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