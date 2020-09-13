using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

namespace PH_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EditMode), "Setup")]
        private static void EditMode_Setup_CreateUI(EditMode __instance, MoveableThumbnailSelectUI ___itemSelectUI)
        {
            Tools.CreateUI(__instance, ___itemSelectUI);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(ThumbnailSelectUI), "SetupCells")]
        private static void CustomSelectListCtrl_SetupCells_GetSelectInfos(List<CustomSelectSet> ___datas)
        {
            if (___datas == null)
                return;

            IEnumerator TranslateItems()
            {
                var cacheDict = Cacher.CacheToDict(Cacher.ReadCache());

                var pushed = 0;
                
                for (var i = 0; i < ___datas.Count; i++)
                {
                    if (cacheDict.ContainsKey(___datas[i].name))
                    {
                        Tools.searchNameStrings[___datas[i].name] = ___datas[i].name + "/v" + cacheDict[___datas[i].name];
                        continue;
                    }

                    var idx = i;
                    TranslationHelper.Translate(___datas[i].name, s => Tools.searchNameStrings[___datas[idx].name] = ___datas[idx].name + "/v" + s);
                    
                    if (pushed++ < 50) 
                        continue;
                    
                    pushed = 0;
                    
                    yield return null;
                }
            }
            
            PH_MakerSearch.instance.StartCoroutine(TranslateItems());
            Cacher.SetCache();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(MoveableThumbnailSelectUI), "UpdateEnables")]
        private static void MoveableThumbnailSelectUI_UpdateEnables_ResetSearch(ThumbnailSelectUI ___select)
        {
            if (___select == PH_MakerSearch.selectUI)
                Tools.ResetSearch();
        }
    }
}