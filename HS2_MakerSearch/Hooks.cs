using System.Linq;
using HarmonyLib;
using CharaCustom;
using UnityEngine.UI;

namespace HS2_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_SetVars(CustomControl __instance)
        {
            Tools.cvss[0] = Singleton<CvsH_Hair>.Instance;
            Tools.cvss[1] = Singleton<CvsC_Clothes>.Instance;
            Tools.cvss[2] = Singleton<CvsA_Slot>.Instance;
            Tools.cvss[3] = Singleton<CvsB_Skin>.Instance;
            Tools.cvss[4] = Singleton<CvsB_Skin>.Instance;
            Tools.cvss[5] = Singleton<CvsB_Sunburn>.Instance;
            Tools.cvss[6] = Singleton<CvsB_Nip>.Instance;
            Tools.cvss[7] = Singleton<CvsB_Underhair>.Instance;
            Tools.cvss[8] = Singleton<CvsB_Paint>.Instance;
            Tools.cvss[9] = Singleton<CvsF_Mole>.Instance;
            Tools.cvss[10] = Singleton<CvsF_EyeLR>.Instance;
            Tools.cvss[11] = Singleton<CvsF_EyeLR>.Instance;
            Tools.cvss[12] = Singleton<CvsF_EyeHL>.Instance;
            Tools.cvss[13] = Singleton<CvsF_Eyebrow>.Instance;
            Tools.cvss[14] = Singleton<CvsF_Eyelashes>.Instance;
            Tools.cvss[15] = Singleton<CvsF_MakeupEyeshadow>.Instance;
            Tools.cvss[16] = Singleton<CvsF_MakeupCheek>.Instance;
            Tools.cvss[17] = Singleton<CvsF_MakeupLip>.Instance;
            Tools.cvss[18] = Singleton<CvsF_MakeupPaint>.Instance;
            Tools.cvss[19] = Singleton<CvsF_FaceType>.Instance;
            Tools.cvss[20] = Singleton<CvsF_FaceType>.Instance;
            
            Tools.controllers[0] = Singleton<CvsH_Hair>.Instance.sscHairType;
            Tools.controllers[1] = Singleton<CvsC_Clothes>.Instance.sscClothesType;
            Tools.controllers[2] = Singleton<CvsA_Slot>.Instance.sscAcs;
            Tools.controllers[3] = Singleton<CvsB_Skin>.Instance.sscSkinType;
            Tools.controllers[4] = Singleton<CvsB_Skin>.Instance.sscDetailType;
            Tools.controllers[5] = Singleton<CvsB_Sunburn>.Instance.sscSunburnType;
            Tools.controllers[6] = Singleton<CvsB_Nip>.Instance.sscNipType;
            Tools.controllers[7] = Singleton<CvsB_Underhair>.Instance.sscUnderhairType;
            Tools.controllers[8] = Singleton<CvsB_Paint>.Instance.sscPaintType;
            Tools.controllers[9] = Singleton<CvsF_Mole>.Instance.sscMole;
            Tools.controllers[10] = Singleton<CvsF_EyeLR>.Instance.sscPupilType;
            Tools.controllers[11] = Singleton<CvsF_EyeLR>.Instance.sscBlackType;
            Tools.controllers[12] = Singleton<CvsF_EyeHL>.Instance.sscEyeHLType;
            Tools.controllers[13] = Singleton<CvsF_Eyebrow>.Instance.sscEyebrowType;
            Tools.controllers[14] = Singleton<CvsF_Eyelashes>.Instance.sscEyelashesType;
            Tools.controllers[15] = Singleton<CvsF_MakeupEyeshadow>.Instance.sscEyeshadowType;
            Tools.controllers[16] = Singleton<CvsF_MakeupCheek>.Instance.sscCheekType;
            Tools.controllers[17] = Singleton<CvsF_MakeupLip>.Instance.sscLipType;
            Tools.controllers[18] = Singleton<CvsF_MakeupPaint>.Instance.sscPaintType;
            Tools.controllers[19] = Singleton<CvsF_FaceType>.Instance.sscSkinType;
            Tools.controllers[20] = Singleton<CvsF_FaceType>.Instance.sscDetailType;
            
            HS2_MakerSearch.sex = __instance.chaCtrl.sex;
            HS2_MakerSearch.lastControllerIdx = -1;

            Tools.CreateUI();
            Cacher.SetupCache();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectScrollController), "CreateList")]
        private static void CustomSelectScrollController_CreateList_TranslateItems(CustomSelectScrollController __instance, CustomSelectScrollController.ScrollData[]  ___scrollerDatas)
        {
            if (___scrollerDatas == null)
                return;

            var anyTranslations = false;
            foreach (var info in ___scrollerDatas.Where(info => !Tools.searchNameStrings.ContainsKey(info.info)))
            {
                if (Cacher.TranslationLookup.TryGetValue(info.info.name, out var tl))
                {
                    Tools.searchNameStrings[info.info] = info.info.name + "/v" + tl;
                    continue;
                }

                var currentInfo = info.info;
                TranslationHelper.Translate(info.info.name, s =>
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
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CvsBase), "ChangeMenuFunc")]
        [HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        public static void Patch_ResetSearch() => Tools.ResetSearch(HS2_MakerSearch.lastControllerIdx);
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsA_Slot), "ChangeMenuFunc")]
        private static void CvsA_Slot_ChangeMenuFunc_ResetSearch(Toggle[] ___tglType)
        {
            var obj = Tools.fields[2].gameObject;
            obj.SetActive(!___tglType[0].isOn);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CvsA_Slot), "ChangeAcsType")]
        private static void CvsA_Slot_ChangeAcsType_ResetSearch(int idx)
        {
            var obj = Tools.fields[2].gameObject;
            obj.SetActive(idx != 0);

            Tools.ResetSearch(2);
        }
    }
}