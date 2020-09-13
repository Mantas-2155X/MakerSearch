using System.Linq;
using System.Collections;

using HarmonyLib;

using CharaCustom;
using SuperScrollView;

using UnityEngine.UI;

namespace AI_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_SetVars(CustomControl __instance)
        {
            AI_MakerSearch.cvsHair = Singleton<CvsH_Hair>.Instance;
            
            AI_MakerSearch.cvsClothes = Singleton<CvsC_Clothes>.Instance;
            
            AI_MakerSearch.cvsAccessories = Singleton<CvsA_Slot>.Instance;
            
            AI_MakerSearch.cvsSkin = Singleton<CvsB_Skin>.Instance;
            AI_MakerSearch.cvsSunburn = Singleton<CvsB_Sunburn>.Instance;
            AI_MakerSearch.cvsNip = Singleton<CvsB_Nip>.Instance;
            AI_MakerSearch.cvsUnderhair = Singleton<CvsB_Underhair>.Instance;
            AI_MakerSearch.cvsPaint = Singleton<CvsB_Paint>.Instance;
            
            AI_MakerSearch.cvsMole = Singleton<CvsF_Mole>.Instance;
            AI_MakerSearch.cvsEye = Singleton<CvsF_EyeLR>.Instance;
            AI_MakerSearch.cvsHighlight = Singleton<CvsF_EyeHL>.Instance;
            AI_MakerSearch.cvsEyebrow = Singleton<CvsF_Eyebrow>.Instance;
            AI_MakerSearch.cvsEyelash = Singleton<CvsF_Eyelashes>.Instance;

            AI_MakerSearch.cvsEyeshadow = Singleton<CvsF_MakeupEyeshadow>.Instance;
            AI_MakerSearch.cvsCheek = Singleton<CvsF_MakeupCheek>.Instance;
            AI_MakerSearch.cvsLip = Singleton<CvsF_MakeupLip>.Instance;
            AI_MakerSearch.cvsFacePaint = Singleton<CvsF_MakeupPaint>.Instance;
            
            AI_MakerSearch.sex = Traverse.Create(__instance).Property("chaCtrl").Property("sex").GetValue<byte>();
            
            Tools.CreateUI();

            // Switch between body Skin and Detail
            AI_MakerSearch.cvsSkin.items[0].tglItem.onValueChanged.AddListener(on => CvsB_Skin_ChangeMenuFunc_SetMainCat());
            AI_MakerSearch.cvsSkin.items[1].tglItem.onValueChanged.AddListener(on => CvsB_Skin_ChangeMenuFunc_SetMainCat());
            
            // Switch between eye Iris and Pupil
            AI_MakerSearch.cvsEye.items[0].tglItem.onValueChanged.AddListener(on => CvsF_EyeLR_ChangeMenuFunc_SetMainCat());
            AI_MakerSearch.cvsEye.items[2].tglItem.onValueChanged.AddListener(on => CvsF_EyeLR_ChangeMenuFunc_SetMainCat());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CustomSelectScrollController), "CreateList")]
        private static void CustomSelectScrollController_CreateList_TranslateItems(CustomSelectScrollController __instance, CustomSelectScrollController.ScrollData[]  ___scrollerDatas)
        {
            if (___scrollerDatas == null)
                return;

            IEnumerator TranslateItems()
            {
                var cacheDict = Cacher.CacheToDict(Cacher.ReadCache());

                var pushed = 0;
                
                foreach (var info in ___scrollerDatas.Where(info => !Tools.searchNameStrings.ContainsKey(info.info)))
                {
                    if (cacheDict.ContainsKey(info.info.name))
                    {
                        Tools.searchNameStrings[info.info] = info.info.name + "/v" + cacheDict[info.info.name];
                        continue;
                    }
                    
                    TranslationHelper.Translate(info.info.name, s => Tools.searchNameStrings[info.info] = info.info.name + "/v" + s);
                    
                    if (pushed++ < 50) 
                        continue;
                    
                    pushed = 0;
                    
                    yield return null;
                }
                
                Cacher.WriteCache();
            }
            
            __instance.StartCoroutine(TranslateItems());
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        private static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            Tools.ResetSearch();
            
            switch (no)
            {
                case 2:
                    AI_MakerSearch.category = Tools.SearchCategory.Hair;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsHair).Field("sscHairType").GetValue<CustomSelectScrollController>();
                    break;
                case 3:
                    AI_MakerSearch.category = Tools.SearchCategory.Clothes;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsClothes).Field("sscClothesType").GetValue<CustomSelectScrollController>();
                    break;
                case 4:
                    AI_MakerSearch.category = Tools.SearchCategory.Accessories;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsAccessories).Field("sscAcs").GetValue<CustomSelectScrollController>();
                    
                    Tools.fields[2].gameObject.SetActive(!Traverse.Create(AI_MakerSearch.cvsAccessories).Field("tglType").GetValue<Toggle[]>()[0].isOn);
                    break;
                case 5:
                    AI_MakerSearch.category = Tools.SearchCategory.Extra;
                    return;
                default:
                    AI_MakerSearch.category = Tools.SearchCategory.None;
                    return;
            }

            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Skin), "ChangeMenuFunc")]
        private static void CvsB_Skin_ChangeMenuFunc_SetMainCat()
        {
            Tools.ResetSearch();
            
            switch (AI_MakerSearch.cvsSkin.GetSelectTab())
            {
                case 0:
                    AI_MakerSearch.category = Tools.SearchCategory.BodySkin;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsSkin).Field("sscSkinType").GetValue<CustomSelectScrollController>();
                    
                    break;
                case 1:
                    AI_MakerSearch.category = Tools.SearchCategory.BodyDetail;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsSkin).Field("sscDetailType").GetValue<CustomSelectScrollController>();
                    
                    break;
            }
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Sunburn), "ChangeMenuFunc")]
        private static void CvsB_Sunburn_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscSunburnType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodySunburn;
            AI_MakerSearch.controller = ___sscSunburnType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Nip), "ChangeMenuFunc")]
        private static void CvsB_Nip_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscNipType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodyNip;
            AI_MakerSearch.controller = ___sscNipType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Underhair), "ChangeMenuFunc")]
        private static void CvsB_Underhair_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscUnderhairType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodyUnderhair;
            AI_MakerSearch.controller = ___sscUnderhairType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Paint), "ChangeMenuFunc")]
        private static void CvsB_Paint_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscPaintType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.BodyPaint;
            AI_MakerSearch.controller = ___sscPaintType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_EyeLR), "ChangeMenuFunc")]
        private static void CvsF_EyeLR_ChangeMenuFunc_SetMainCat()
        {
            Tools.ResetSearch();
            
            // Reversed 2 and 0 because of whatever reason it doesn't work
            switch (AI_MakerSearch.cvsEye.GetSelectTab())
            {
                case 2:
                    AI_MakerSearch.category = Tools.SearchCategory.FaceEyeIris;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsEye).Field("sscBlackType").GetValue<CustomSelectScrollController>();
                    
                    break;
                case 0:
                    AI_MakerSearch.category = Tools.SearchCategory.FaceEyePupil;
                    AI_MakerSearch.controller = Traverse.Create(AI_MakerSearch.cvsEye).Field("sscPupilType").GetValue<CustomSelectScrollController>();
                    
                    break;
            }
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_EyeHL), "ChangeMenuFunc")]
        private static void CvsF_EyeHL_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyeHLType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FaceHighlight;
            AI_MakerSearch.controller = ___sscEyeHLType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_Eyebrow), "ChangeMenuFunc")]
        private static void CvsF_Eyebrow_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyebrowType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FaceEyebrow;
            AI_MakerSearch.controller = ___sscEyebrowType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_Eyelashes), "ChangeMenuFunc")]
        private static void CvsF_Eyelashes_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyelashesType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FaceEyelash;
            AI_MakerSearch.controller = ___sscEyelashesType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_Mole), "ChangeMenuFunc")]
        private static void CvsF_Mole_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscMole)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FaceMole;
            AI_MakerSearch.controller = ___sscMole;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupCheek), "ChangeMenuFunc")]
        private static void CvsF_MakeupCheek_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscCheekType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FaceCheek;
            AI_MakerSearch.controller = ___sscCheekType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupEyeshadow), "ChangeMenuFunc")]
        private static void CvsF_MakeupEyeshadow_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyeshadowType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FaceEyeshadow;
            AI_MakerSearch.controller = ___sscEyeshadowType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupLip), "ChangeMenuFunc")]
        private static void CvsF_MakeupLip_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscLipType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FaceLip;
            AI_MakerSearch.controller = ___sscLipType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupPaint), "ChangeMenuFunc")]
        private static void CvsF_MakeupPaint_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscPaintType)
        {
            Tools.ResetSearch();
            
            AI_MakerSearch.category = Tools.SearchCategory.FacePaint;
            AI_MakerSearch.controller = ___sscPaintType;
            
            AI_MakerSearch.view = AI_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsBase), "ChangeMenuFunc")]
        private static void CvsBase_ChangeMenuFunc_ResetSearch() => Tools.ResetSearch();
        
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

            Tools.ResetSearch();
        }
    }
}