using System;
using System.Linq;

using AIChara;
using CharaCustom;

using UnityEngine;
using UnityEngine.UI;

using HarmonyLib;
using XUnity.AutoTranslator.Plugin.Core;

namespace AI_MakerSearch
{
    public static class Tools
    {
        public static readonly InputField[] fields = new InputField[9];
        
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
        };
 
        public static void CreateUI()
        {
            var orig = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_ShapeWhole/Scroll View/Viewport/Content/SliderSet/SldInputField");

            var i = 0;
            foreach (var targetStr in targets)
            {
                var target = GameObject.Find(targetStr);
                
                if (i == 1 && AI_MakerSearch.isSteam)
                {
                    var color = target.transform.Find("DefaultColor");
                    var rectC = color.GetComponent<RectTransform>();
                    rectC.offsetMax = new Vector2(200, rectC.offsetMax.y);
                    
                    var button = color.Find("Button");
                    button.GetComponent<RectTransform>().offsetMax = new Vector2(200, 0);
                }
                
                var cp = UnityEngine.Object.Instantiate(orig, target.transform);
                cp.name = "Search";

                var placeholderObj = cp.transform.Find("Placeholder");
                var placeholder = placeholderObj.GetComponent<Text>();
                placeholder.text = "Search";

                UnityEngine.Object.Destroy(cp.transform.Find("SldInputField Input Caret"));

                var rect = cp.GetComponent<RectTransform>();

                if (i == 1) // Clothes
                {
                    rect.offsetMin = new Vector2(-250, 3);
                    rect.offsetMax = new Vector2(0, -383);
                } 
                else
                {
                    rect.offsetMin = new Vector2(AI_MakerSearch.isSteam ? -452 : -420, 3);
                    rect.offsetMax = new Vector2(0, -383);

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
                        AutoTranslator.Default.TranslateAsync(data.name, result => { searchIn = result.Succeeded ? result.TranslatedText : data.name; });

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
                case SearchCategory.Face:
                    return false;
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
            if (AI_MakerSearch.searchString == "") 
                return;
            
            UpdateUI(AI_MakerSearch.category);
            AI_MakerSearch.searchString = "";

            foreach (var field in fields.Where(field => field != null))
                field.text = "";
        }
        
        public enum SearchBy
        {
            Name,
            AssetBundle
        }

        public enum SearchCategory
        {
            Face,
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