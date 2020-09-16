using System;
using System.Linq;
using System.Collections.Generic;

using AIChara;
using CharaCustom;

using UnityEngine;
using UnityEngine.UI;

using HarmonyLib;

namespace AI_MakerSearch
{
    public static class Tools
    {
        public static readonly InputField[] fields = new InputField[19];
        public static readonly Dictionary<CustomSelectInfo, string> searchNameStrings = new Dictionary<CustomSelectInfo, string>();

        private static readonly string[] targets =
        {
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinHair/H_Hair/Setting/Setting01",                      // Hair
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting01",     // Clothes
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinAccessory/A_Slot/Setting/Setting01",                 // Accessories
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Skin/Setting/Setting01",                      // Body Skin
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Skin/Setting/Setting02",                      // Body Detail
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Sunburn/Setting/Setting01",                   // Body Sunburn
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Nip/Setting/Setting01",                       // Body Nip
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Underhair/Setting/Setting01",                 // Body Underhair
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Paint/Setting/Setting01",                     // Body Paint
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_Mole/Setting/Setting01",                      // Face Mole
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_EyeLR/Setting/Setting01",                     // Face Eye Iris
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_EyeLR/Setting/Setting03",                     // Face Eye Pupil
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_EyeHL/Setting/Setting01",                     // Face Highlight
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_Eyebrow/Setting/Setting01",                   // Face Eyebrow
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_Eyelashes/Setting/Setting01",                 // Face Eyelash
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupEyeshadow/Setting/Setting01",           // Face Eyeshadow
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupCheek/Setting/Setting01",               // Face Cheek
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupLip/Setting/Setting01",                 // Face Lip
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupPaint/Setting/Setting01",               // Face Paint
        };
 
        public static void CreateUI()
        {
            var orig = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_ShapeWhole/Scroll View/Viewport/Content/SliderSet/SldInputField");
            var resetButton = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting01/DefaultColor");

            var i = 0;
            foreach (var targetStr in targets)
            {
                var target = GameObject.Find(targetStr);
                
                if (i == 1 && AI_MakerSearch.isSteam)
                {
                    var color = target.transform.Find("DefaultColor");
                    var rectC = color.GetComponent<RectTransform>();
                    rectC.offsetMax = new Vector2(200, rectC.offsetMax.y);
                    
                    var cButton = color.Find("Button");
                    cButton.GetComponent<RectTransform>().offsetMax = new Vector2(200, 0);
                }
                
                var cp = UnityEngine.Object.Instantiate(orig, target.transform);
                cp.name = "Search";

                var placeholderObj = cp.transform.Find("Placeholder");
                var placeholder = placeholderObj.GetComponent<Text>();
                placeholder.text = "Search";

                UnityEngine.Object.Destroy(cp.transform.Find("SldInputField Input Caret"));

                var rect = cp.GetComponent<RectTransform>();

                var resetCopy = UnityEngine.Object.Instantiate(resetButton, target.transform);
                resetCopy.name = "Reset";
            
                var resetRect = resetCopy.GetComponent<RectTransform>();
                resetRect.offsetMin = new Vector2(!AI_MakerSearch.isSteam ? 365 : 396, -420);
                resetRect.offsetMax = new Vector2(!AI_MakerSearch.isSteam ? 615 : 646, -440);

                var resetText = resetCopy.GetComponentInChildren<Text>();
                resetText.text = "Reset";
                
                if (i == 1) // Clothes
                {
                    rect.offsetMin = new Vector2(!AI_MakerSearch.isSteam ? -255 : -250, 3);
                    rect.offsetMax = new Vector2(-60, -383);
                } 
                else
                {
                    rect.offsetMin = new Vector2(AI_MakerSearch.isSteam ? -452 : -420, 3);
                    rect.offsetMax = new Vector2(-60, -383);

                    var box = target.transform.Find("SelectBox");
                    var scrollview = box.Find("Scroll View");
                    
                    box.GetComponent<RectTransform>().offsetMin = new Vector2(0, -372);
                    scrollview.GetComponent<RectTransform>().offsetMin = new Vector2(0, i == 2 ? -264 : i == 4 ? -332 : -372);
                }

                var input = cp.GetComponent<InputField>();
                input.contentType = InputField.ContentType.Standard;
                input.characterLimit = 64;

                input.onValueChanged.RemoveAllListeners();
                input.onEndEdit.RemoveAllListeners();

                input.textComponent.text = "";
                input.text = "";

                input.onValueChanged.AddListener(delegate(string text) { placeholder.enabled = text == ""; });
                input.onEndEdit.AddListener(delegate(string text)
                {
                    AI_MakerSearch.searchString = text;
                    AI_MakerSearch.Search();
                });

                var button = resetCopy.GetComponentInChildren<Button>();

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(delegate
                {
                    input.text = "";
                    AI_MakerSearch.searchString = "";
                    AI_MakerSearch.Search();
                });

                var buttonRect = button.GetComponent<RectTransform>();
                buttonRect.offsetMax = new Vector2(60, 60);
                
                fields[i] = input;

                if (i == 2)
                    cp.SetActive(false);
                
                i++;
            }
        }

        public static bool ItemMatchesSearch(CustomSelectInfo data, string searchStr)
        {
            var searchIn = "";

            switch (AI_MakerSearch.searchBy.Value)
            {
                case SearchBy.Name:
                    searchIn = data.name;
                    
                    if (AI_MakerSearch.useTranslatedCache.Value)
                        searchIn = searchNameStrings.TryGetValue(data, out var cachedTranslation) ? cachedTranslation : data.name;

                    break;
                case SearchBy.AssetBundle:
                    searchIn = data.assetBundle;
                    break;
            }

            var rule = StringComparison.Ordinal;
            if (!AI_MakerSearch.caseSensitive.Value)
            {
                searchStr = searchStr.ToLowerInvariant();
                rule = StringComparison.OrdinalIgnoreCase;
            }

            var splitSearchStr = searchStr.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            return splitSearchStr.All(s => searchIn.IndexOf(s, rule) >= 0);
        }
        
        public static bool UpdateUI(SearchCategory category)
        {
            switch (category)
            {
                case SearchCategory.FaceEyeIris:
                    var listFIris = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyeblack);
                    Traverse.Create(AI_MakerSearch.cvsEye).Field("sscBlackType").Method("CreateList", listFIris).GetValue();

                    AI_MakerSearch.cvsEye.UpdateCustomUI();
                    break;
                case SearchCategory.FaceEyePupil:
                    var listFPupil = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eye);
                    Traverse.Create(AI_MakerSearch.cvsEye).Field("sscPupilType").Method("CreateList", listFPupil).GetValue();

                    AI_MakerSearch.cvsEye.UpdateCustomUI();
                    break;
                case SearchCategory.FaceMole:
                    var listFMole = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_mole);
                    Traverse.Create(AI_MakerSearch.cvsMole).Field("sscMole").Method("CreateList", listFMole).GetValue();

                    AI_MakerSearch.cvsMole.UpdateCustomUI();
                    break;
                case SearchCategory.FaceHighlight:
                    var listFHighlight = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eye_hl);
                    Traverse.Create(AI_MakerSearch.cvsHighlight).Field("sscEyeHLType").Method("CreateList", listFHighlight).GetValue();

                    AI_MakerSearch.cvsHighlight.UpdateCustomUI();
                    break;
                case SearchCategory.FaceEyebrow:
                    var listFEyebrow = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyebrow);
                    Traverse.Create(AI_MakerSearch.cvsEyebrow).Field("sscEyebrowType").Method("CreateList", listFEyebrow).GetValue();

                    AI_MakerSearch.cvsEyebrow.UpdateCustomUI();
                    break;
                case SearchCategory.FaceEyelash:
                    var listFEyelash = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyelash);
                    Traverse.Create(AI_MakerSearch.cvsEyelash).Field("sscEyelashesType").Method("CreateList", listFEyelash).GetValue();

                    AI_MakerSearch.cvsEyelash.UpdateCustomUI();
                    break;
                case SearchCategory.FaceEyeshadow:
                    var listFEyeshadow = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyeshadow);
                    Traverse.Create(AI_MakerSearch.cvsEyeshadow).Field("sscEyeshadowType").Method("CreateList", listFEyeshadow).GetValue();

                    AI_MakerSearch.cvsEyelash.UpdateCustomUI();
                    break;
                case SearchCategory.FaceCheek:
                    var listFCheek = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_cheek);
                    Traverse.Create(AI_MakerSearch.cvsCheek).Field("sscCheekType").Method("CreateList", listFCheek).GetValue();

                    AI_MakerSearch.cvsEyelash.UpdateCustomUI();
                    break;
                case SearchCategory.FaceLip:
                    var listFLip = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_lip);
                    Traverse.Create(AI_MakerSearch.cvsLip).Field("sscLipType").Method("CreateList", listFLip).GetValue();

                    AI_MakerSearch.cvsEyelash.UpdateCustomUI();
                    break;
                case SearchCategory.FacePaint:
                    var listFPaint = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_paint);
                    Traverse.Create(AI_MakerSearch.cvsFacePaint).Field("sscPaintType").Method("CreateList", listFPaint).GetValue();

                    AI_MakerSearch.cvsEyelash.UpdateCustomUI();
                    break;
                case SearchCategory.BodySkin:
                    var listBSkin = CvsBase.CreateSelectList(AI_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_skin_b : ChaListDefine.CategoryNo.ft_skin_b);
                    Traverse.Create(AI_MakerSearch.cvsSkin).Field("sscSkinType").Method("CreateList", listBSkin).GetValue();

                    AI_MakerSearch.cvsSkin.UpdateCustomUI();
                    break;
                case SearchCategory.BodyDetail:
                    var listBDetail = CvsBase.CreateSelectList(AI_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_detail_b : ChaListDefine.CategoryNo.ft_detail_b);
                    Traverse.Create(AI_MakerSearch.cvsSkin).Field("sscDetailType").Method("CreateList", listBDetail).GetValue();

                    AI_MakerSearch.cvsSkin.UpdateCustomUI();
                    break;
                case SearchCategory.BodySunburn:
                    var listBSunburn = CvsBase.CreateSelectList(AI_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_sunburn : ChaListDefine.CategoryNo.ft_sunburn);
                    Traverse.Create(AI_MakerSearch.cvsSunburn).Field("sscSunburnType").Method("CreateList", listBSunburn).GetValue();

                    AI_MakerSearch.cvsSunburn.UpdateCustomUI();
                    break;
                case SearchCategory.BodyNip:
                    var listBNip = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_nip);
                    Traverse.Create(AI_MakerSearch.cvsNip).Field("sscNipType").Method("CreateList", listBNip).GetValue();

                    AI_MakerSearch.cvsNip.UpdateCustomUI();
                    break;
                case SearchCategory.BodyUnderhair:
                    var listBUnderhair = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_underhair);
                    Traverse.Create(AI_MakerSearch.cvsUnderhair).Field("sscUnderhairType").Method("CreateList", listBUnderhair).GetValue();

                    AI_MakerSearch.cvsUnderhair.UpdateCustomUI();
                    break;
                case SearchCategory.BodyPaint:
                    var listBPaint = CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_paint);
                    Traverse.Create(AI_MakerSearch.cvsPaint).Field("sscPaintType").Method("CreateList", listBPaint).GetValue();

                    AI_MakerSearch.cvsPaint.UpdateCustomUI();
                    break;
                case SearchCategory.Hair:
                    AI_MakerSearch.cvsHair.UpdateHairList();
                    AI_MakerSearch.cvsHair.UpdateCustomUI();
                    break;
                case SearchCategory.Clothes:
                    AI_MakerSearch.cvsClothes.UpdateClothesList();
                    AI_MakerSearch.cvsClothes.UpdateCustomUI();
                    break;
                case SearchCategory.Accessories:
                    AI_MakerSearch.cvsAccessories.UpdateAcsList();
                    AI_MakerSearch.cvsAccessories.UpdateCustomUI();
                    break;
                case SearchCategory.Extra:
                    return false;
                case SearchCategory.None:
                    return false;
                default:
                    return false;
            }

            return true;
        }

        public static void ResetSearch()
        {
            if (AI_MakerSearch.searchTextMemory.Value == SearchTextMemory.None)
                foreach (var field in fields.Where(field => field != null))
                    field.text = "";

            if (AI_MakerSearch.searchString == "") 
                return;
            
            UpdateUI(AI_MakerSearch.category);
            
            AI_MakerSearch.searchString = "";
        }

        public enum SearchTextMemory
        {
            Separate,
            Global,
            None
        }
        
        public enum SearchBy
        {
            Name,
            AssetBundle
        }

        public enum SearchCategory
        {
            FaceMole,
            FaceEyePupil,
            FaceEyeIris,
            FaceHighlight,
            FaceEyebrow,
            FaceEyelash,
            FaceEyeshadow,
            FaceCheek,
            FaceLip,
            FacePaint,
            BodySkin,
            BodyDetail,
            BodySunburn,
            BodyNip,
            BodyUnderhair,
            BodyPaint,
            Hair,
            Clothes,
            Accessories,
            Extra,
            None
        }
    }
}