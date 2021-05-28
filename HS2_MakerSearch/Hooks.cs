using System;
using System.Linq;
using System.Timers;

using BepInEx;
using HarmonyLib;

using CharaCustom;
using SuperScrollView;

using UniRx;
using UniRx.Triggers;

using UnityEngine.UI;

namespace HS2_MakerSearch
{
    public static class Hooks
    {
        [HarmonyPostfix, HarmonyPatch(typeof(CustomControl), "Initialize")]
        private static void CustomControl_Initialize_SetVars(CustomControl __instance)
        {
            HS2_MakerSearch.cvsHair = Singleton<CvsH_Hair>.Instance;
            
            HS2_MakerSearch.cvsClothes = Singleton<CvsC_Clothes>.Instance;
            
            HS2_MakerSearch.cvsAccessories = Singleton<CvsA_Slot>.Instance;
            
            HS2_MakerSearch.cvsSkin = Singleton<CvsB_Skin>.Instance;
            HS2_MakerSearch.cvsSunburn = Singleton<CvsB_Sunburn>.Instance;
            HS2_MakerSearch.cvsNip = Singleton<CvsB_Nip>.Instance;
            HS2_MakerSearch.cvsUnderhair = Singleton<CvsB_Underhair>.Instance;
            HS2_MakerSearch.cvsPaint = Singleton<CvsB_Paint>.Instance;
            
            HS2_MakerSearch.cvsMole = Singleton<CvsF_Mole>.Instance;
            HS2_MakerSearch.cvsEye = Singleton<CvsF_EyeLR>.Instance;
            HS2_MakerSearch.cvsHighlight = Singleton<CvsF_EyeHL>.Instance;
            HS2_MakerSearch.cvsEyebrow = Singleton<CvsF_Eyebrow>.Instance;
            HS2_MakerSearch.cvsEyelash = Singleton<CvsF_Eyelashes>.Instance;

            HS2_MakerSearch.cvsEyeshadow = Singleton<CvsF_MakeupEyeshadow>.Instance;
            HS2_MakerSearch.cvsCheek = Singleton<CvsF_MakeupCheek>.Instance;
            HS2_MakerSearch.cvsLip = Singleton<CvsF_MakeupLip>.Instance;
            HS2_MakerSearch.cvsFacePaint = Singleton<CvsF_MakeupPaint>.Instance;
            
            HS2_MakerSearch.sex = Traverse.Create(__instance).Property("chaCtrl").Property("sex").GetValue<byte>();
            
            Tools.CreateUI();

            // Switch between body Skin and Detail
            HS2_MakerSearch.cvsSkin.items[0].tglItem.onValueChanged.AddListener(on => CvsB_Skin_ChangeMenuFunc_SetMainCat());
            HS2_MakerSearch.cvsSkin.items[1].tglItem.onValueChanged.AddListener(on => CvsB_Skin_ChangeMenuFunc_SetMainCat());
            
            // Switch between eye Iris and Pupil
            HS2_MakerSearch.cvsEye.items[0].tglItem.onValueChanged.AddListener(on => CvsF_EyeLR_ChangeMenuFunc_SetMainCat());
            HS2_MakerSearch.cvsEye.items[2].tglItem.onValueChanged.AddListener(on => CvsF_EyeLR_ChangeMenuFunc_SetMainCat());
            
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
            CustomBase.Instance.OnDestroyAsObservable().Subscribe(_ =>
            {
                if (_cacheSaveTimer.Enabled) OnSave(null, null);
            });
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
                _cacheSaveTimer.Stop();
                _cacheSaveTimer.Start();
            }
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CustomChangeMainMenu), "ChangeWindowSetting")]
        private static void CustomChangeMainMenu_ChangeWindowSetting_SetMainCat(int no)
        {
            Tools.ResetSearch();
            
            switch (no)
            {
                case 2:
                    HS2_MakerSearch.category = Tools.SearchCategory.Hair;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsHair).Field("sscHairType").GetValue<CustomSelectScrollController>();
                    break;
                case 3:
                    HS2_MakerSearch.category = Tools.SearchCategory.Clothes;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsClothes).Field("sscClothesType").GetValue<CustomSelectScrollController>();
                    break;
                case 4:
                    HS2_MakerSearch.category = Tools.SearchCategory.Accessories;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsAccessories).Field("sscAcs").GetValue<CustomSelectScrollController>();
                    
                    Tools.fields[2].gameObject.SetActive(!Traverse.Create(HS2_MakerSearch.cvsAccessories).Field("tglType").GetValue<Toggle[]>()[0].isOn);
                    break;
                case 5:
                    HS2_MakerSearch.category = Tools.SearchCategory.Extra;
                    return;
                default:
                    HS2_MakerSearch.category = Tools.SearchCategory.None;
                    return;
            }

            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Skin), "ChangeMenuFunc")]
        private static void CvsB_Skin_ChangeMenuFunc_SetMainCat()
        {
            Tools.ResetSearch();
            
            switch (HS2_MakerSearch.cvsSkin.GetSelectTab())
            {
                case 0:
                    HS2_MakerSearch.category = Tools.SearchCategory.BodySkin;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsSkin).Field("sscSkinType").GetValue<CustomSelectScrollController>();
                    
                    break;
                case 1:
                    HS2_MakerSearch.category = Tools.SearchCategory.BodyDetail;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsSkin).Field("sscDetailType").GetValue<CustomSelectScrollController>();
                    
                    break;
            }
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Sunburn), "ChangeMenuFunc")]
        private static void CvsB_Sunburn_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscSunburnType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodySunburn;
            HS2_MakerSearch.controller = ___sscSunburnType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Nip), "ChangeMenuFunc")]
        private static void CvsB_Nip_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscNipType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodyNip;
            HS2_MakerSearch.controller = ___sscNipType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Underhair), "ChangeMenuFunc")]
        private static void CvsB_Underhair_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscUnderhairType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodyUnderhair;
            HS2_MakerSearch.controller = ___sscUnderhairType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsB_Paint), "ChangeMenuFunc")]
        private static void CvsB_Paint_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscPaintType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.BodyPaint;
            HS2_MakerSearch.controller = ___sscPaintType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_EyeLR), "ChangeMenuFunc")]
        private static void CvsF_EyeLR_ChangeMenuFunc_SetMainCat()
        {
            Tools.ResetSearch();
            
            // Reversed 2 and 0 because of whatever reason it doesn't work
            switch (HS2_MakerSearch.cvsEye.GetSelectTab())
            {
                case 2:
                    HS2_MakerSearch.category = Tools.SearchCategory.FaceEyeIris;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsEye).Field("sscBlackType").GetValue<CustomSelectScrollController>();
                    
                    break;
                case 0:
                    HS2_MakerSearch.category = Tools.SearchCategory.FaceEyePupil;
                    HS2_MakerSearch.controller = Traverse.Create(HS2_MakerSearch.cvsEye).Field("sscPupilType").GetValue<CustomSelectScrollController>();
                    
                    break;
            }
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_EyeHL), "ChangeMenuFunc")]
        private static void CvsF_EyeHL_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyeHLType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FaceHighlight;
            HS2_MakerSearch.controller = ___sscEyeHLType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_Eyebrow), "ChangeMenuFunc")]
        private static void CvsF_Eyebrow_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyebrowType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FaceEyebrow;
            HS2_MakerSearch.controller = ___sscEyebrowType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_Eyelashes), "ChangeMenuFunc")]
        private static void CvsF_Eyelashes_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyelashesType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FaceEyelash;
            HS2_MakerSearch.controller = ___sscEyelashesType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_Mole), "ChangeMenuFunc")]
        private static void CvsF_Mole_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscMole)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FaceMole;
            HS2_MakerSearch.controller = ___sscMole;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupCheek), "ChangeMenuFunc")]
        private static void CvsF_MakeupCheek_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscCheekType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FaceCheek;
            HS2_MakerSearch.controller = ___sscCheekType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupEyeshadow), "ChangeMenuFunc")]
        private static void CvsF_MakeupEyeshadow_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscEyeshadowType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FaceEyeshadow;
            HS2_MakerSearch.controller = ___sscEyeshadowType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupLip), "ChangeMenuFunc")]
        private static void CvsF_MakeupLip_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscLipType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FaceLip;
            HS2_MakerSearch.controller = ___sscLipType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(CvsF_MakeupPaint), "ChangeMenuFunc")]
        private static void CvsF_MakeupPaint_ChangeMenuFunc_SetMainCat(CustomSelectScrollController ___sscPaintType)
        {
            Tools.ResetSearch();
            
            HS2_MakerSearch.category = Tools.SearchCategory.FacePaint;
            HS2_MakerSearch.controller = ___sscPaintType;
            
            HS2_MakerSearch.view = HS2_MakerSearch.controller.GetComponent<LoopListView2>();
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