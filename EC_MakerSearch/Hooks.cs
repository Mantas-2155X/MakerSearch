using System.Linq;
using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

using ChaCustom;

namespace EC_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_CreateUI()
        {
            EC_MakerSearch.ctrl = null;
            
            Tools.disvisibleMemory.Clear();
            Tools.CreateUI();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "Create")]
        private static void CustomSelectListCtrl_Create_GetSelectInfos(CustomSelectListCtrl __instance, List<CustomSelectInfo> ___lstSelectInfo)
        {
            if (___lstSelectInfo == null)
                return;

            IEnumerator TranslateItems()
            {
                var cacheDict = Cacher.CacheToDict(Cacher.ReadCache());

                var pushed = 0;
                
                foreach (var info in ___lstSelectInfo.Where(info => !Tools.searchNameStrings.ContainsKey(info)))
                {
                    if (cacheDict.ContainsKey(info.name))
                    {
                        Tools.searchNameStrings[info] = info.name + "/v" + cacheDict[info.name];
                        continue;
                    }
                    
                    TranslationHelper.Translate(info.name, s => Tools.searchNameStrings[info] = info.name + "/v" + s);
                    
                    if (pushed++ < 50) 
                        continue;
                    
                    pushed = 0;
                    
                    yield return null;
                }
                
                Cacher.WriteCache();
            }
            
            __instance.StartCoroutine(TranslateItems());
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectListCtrl), "Update")]
        private static void CustomSelectListCtrl_Update_ChangeController(CustomSelectListCtrl __instance)
        {
            if (EC_MakerSearch.ctrl == __instance) 
                return;

            if (__instance.canvasGrp == null)
                return;

            if (!__instance.canvasGrp[0].name.Contains("win") || !__instance.canvasGrp[0].interactable || !__instance.canvasGrp[1].interactable || !__instance.canvasGrp[2].interactable)
                return;
            
            Tools.ResetSearch();
            
            EC_MakerSearch.ctrl = __instance;
            Tools.RememberDisvisibles();
        }
    }
}