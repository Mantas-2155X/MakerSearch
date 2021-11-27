using System;
using System.Linq;
using System.Collections.Generic;

using AIChara;
using CharaCustom;
using Sideloader.AutoResolver;
using UnityEngine;
using UnityEngine.UI;

namespace AI_MakerSearch
{
    public static class Tools
    {
        public static readonly CustomSelectScrollController[] controllers = new CustomSelectScrollController[21];

        public static readonly CvsBase[] cvss = new CvsBase[21];
        public static readonly InputField[] fields = new InputField[21];

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
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_FaceType/Setting/Setting02",                  // Face Skin
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_FaceType/Setting/Setting03",                  // Face Wrinkles
        };
 
        public static void CreateUI()
        {
            var orig = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_ShapeWhole/Scroll View/Viewport/Content/SliderSet/SldInputField");
            var resetButton = GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting01/DefaultColor");

            for (var i = 0; i < targets.Length; i++)
            {
                var idx = i;

                var target = GameObject.Find(targets[i]);

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

                var caret = cp.transform.Find("SldInputField Input Caret");
                if (caret != null)
                    UnityEngine.Object.Destroy(caret.gameObject);

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
                    scrollview.GetComponent<RectTransform>().offsetMin = new Vector2(0, i == 2 ? -264 : i == 4 || i == 20 ? -332 : -372);
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
                    AI_MakerSearch.Search(idx);
                });

                var button = resetCopy.GetComponentInChildren<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(delegate
                {
                    input.text = "";
                    AI_MakerSearch.searchString = "";
                    AI_MakerSearch.Search(idx);
                });

                var buttonRect = button.GetComponent<RectTransform>();
                buttonRect.offsetMax = new Vector2(60, 60);

                fields[i] = input;
                
                if (i == 2)
                    cp.SetActive(false);
            }
        }

        public static bool ItemMatchesSearch(CustomSelectInfo data, string searchStr)
        {
            var searchIn = "";

            if (AI_MakerSearch.searchName.Value)
            {
                searchIn = searchIn + " " + data.name;
                    
                if (AI_MakerSearch.useTranslatedCache.Value)
                    searchIn = searchIn + " " + (searchNameStrings.TryGetValue(data, out var cachedTranslation) ? cachedTranslation : data.name);
            }
            
            if (AI_MakerSearch.searchAssetBundle.Value)
            {
                searchIn = searchIn + " " + data.assetBundle;
            }
            
            if (AI_MakerSearch.searchAuthor.Value)
            {
                var info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)data.category, data.id);
                if (info != null)
                {
                    var manifest = Sideloader.Sideloader.GetManifest(info.GUID);
                    if (manifest?.Author != null)
                        searchIn = searchIn + " " + manifest.Author;
                }
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
        
        public static bool UpdateUI(int controllerIdx)
        {
            if (controllerIdx == -1)
                return false;
            
            if (controllerIdx == 0)
            {
                var cvs = (CvsH_Hair)cvss[controllerIdx];
                cvs.UpdateHairList();
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 1)
            {
                var cvs = (CvsC_Clothes)cvss[controllerIdx];
                cvs.UpdateClothesList();
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 2)
            {
                var cvs = (CvsA_Slot)cvss[controllerIdx];
                cvs.UpdateAcsList();
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 3)
            {
                var cvs = (CvsB_Skin)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(AI_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_skin_b: ChaListDefine.CategoryNo.ft_skin_b));
                cvs.UpdateCustomUI();            
            }
            else if (controllerIdx == 4)
            {
                var cvs = (CvsB_Skin)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(AI_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_detail_b: ChaListDefine.CategoryNo.ft_detail_b));
                cvs.UpdateCustomUI();            
            }
            else if (controllerIdx == 5)
            {
                var cvs = (CvsB_Sunburn)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(AI_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_sunburn : ChaListDefine.CategoryNo.ft_sunburn));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 6)
            {
                var cvs = (CvsB_Nip)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_nip));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 7)
            {
                var cvs = (CvsB_Underhair)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_underhair));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 8)
            {
                var cvs = (CvsF_Mole)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_mole));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 9)
            {
                var cvs = (CvsF_Mole)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_mole));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 10)
            {
                var cvs = (CvsF_EyeLR)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eye));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 11)
            {
                var cvs = (CvsF_EyeLR)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyeblack));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 12)
            {
                var cvs = (CvsF_EyeHL)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eye_hl));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 13)
            {
                var cvs = (CvsF_Eyebrow)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyebrow));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 14)
            {
                var cvs = (CvsF_Eyelashes)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyelash));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 15)
            {
                var cvs = (CvsF_MakeupEyeshadow)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_eyeshadow));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 16)
            {
                var cvs = (CvsF_MakeupEyeshadow)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_cheek));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 17)
            {
                var cvs = (CvsF_MakeupLip)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_lip));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 18)
            {
                var cvs = (CvsF_MakeupPaint)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(ChaListDefine.CategoryNo.st_paint));
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 19)
            {
                var cvs = (CvsF_FaceType)cvss[controllerIdx];
                cvs.UpdateSkinList();
                cvs.UpdateCustomUI();
            }
            else if (controllerIdx == 20)
            {
                var cvs = (CvsF_FaceType)cvss[controllerIdx];
                var controller = controllers[controllerIdx];
                
                controller.CreateList(CvsBase.CreateSelectList(AI_MakerSearch.sex == 0 ? ChaListDefine.CategoryNo.mt_detail_f : ChaListDefine.CategoryNo.ft_detail_f));
                cvs.UpdateCustomUI();
            }

            return true;
        }

        public static void ResetSearch(int controllerIdx)
        {
            if (AI_MakerSearch.searchTextMemory.Value == SearchTextMemory.None)
                foreach (var field in fields.Where(field => field != null))
                    field.text = "";

            UpdateUI(controllerIdx);
            
            AI_MakerSearch.searchString = "";
            AI_MakerSearch.lastControllerIdx = -1;
        }
        
        public enum SearchTextMemory
        {
            Separate,
            Global,
            None
        }
    }
}