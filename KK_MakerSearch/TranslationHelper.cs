using System;
using System.Linq;
using BepInEx;
using XUnity.AutoTranslator.Plugin.Core;

namespace KK_MakerSearch
{
    public static class TranslationHelper // "Borrowed" from ManlyMarco <3
    {
        private static readonly Action<string, Action<string>> _translatorCallback;

        static TranslationHelper()
        {
            var xua = Type.GetType("XUnity.AutoTranslator.Plugin.Core.ITranslator, XUnity.AutoTranslator.Plugin.Core", false);
            if (xua != null && xua.GetMethods().Any(x => x.Name == "TranslateAsync"))
            {
                _translatorCallback = (s, action) =>
                {
                    // The lambda doesn't get its types resolved until it's called so this doesn't crash here if the type doesn't exist
                    AutoTranslator.Default.TranslateAsync(s, result => { if (result.Succeeded) action(result.TranslatedText); });
                };
            }
            else
            {
                var logger = new BepInEx.Logging.ManualLogSource("MakerSearch");
                logger.LogWarning("Could not find method AutoTranslator.Default.TranslateAsync, item translations will be limited or unavailable");
                _translatorCallback = null;
            }
        }

        public static void Translate(string input, Action<string> updateAction)
        {
            if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));
            
            // Make sure there's a valid value set in case we need to wait
            updateAction(input);

            if (_translatorCallback != null)
            {
                // XUA needs to run on the main thread
                //ThreadingHelper.Instance.StartSyncInvoke(() => _translatorCallback(input, updateAction));
                _translatorCallback(input, updateAction);
            }
        }
    }
}