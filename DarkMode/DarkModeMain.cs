using HarmonyLib;
using UnityEngine;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using AmongUs.Data;
using InnerNet;
using TMPro;

[BepInPlugin("com.darkmode.techiee", "DarkMode", "1.1.5")]
public class DarkModePlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony("Dark Mode, by Techiee.");
    public static ConfigEntry<bool> DarkModeConfig;
    public static ConfigEntry<bool> ShowWatermark;
    public override void Load()
    {
        DarkModeConfig = Config.Bind("DarkMode",
                                     "DarkMode",
                                     true,
                                     "Set this to false if you don't want dark mode for now");

        ShowWatermark = Config.Bind("Credits",
                                     "ShowWatermark?",
                                     true,
                                     "Set this to false if you don't want to see the watermark.");
        Harmony.PatchAll();
    }
}

namespace DarkMode
{ 
    [HarmonyPatch(typeof(ChatBubble))]
    public static class ChatBubblePatch
    {
        public static string ColorString(Color32 color, string str) => $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";

        [HarmonyPatch(nameof(ChatBubble.SetText)), HarmonyPrefix]
        public static void SetText_Prefix(ChatBubble __instance, ref string chatText)
        {
            var sr = __instance.transform.Find("Background").GetComponent<SpriteRenderer>();
            if (DarkModePlugin.DarkModeConfig.Value) sr.color = new Color(0, 0, 0, 128);

            if (chatText.Contains("░") ||
                chatText.Contains("▄") ||
                chatText.Contains("█") ||
                chatText.Contains("▌") ||
                chatText.Contains("▒")) ;
            else
            {
                if (DarkModePlugin.DarkModeConfig.Value) chatText = ColorString(Color.white, chatText.TrimEnd('\0'));
                else chatText = ColorString(Color.black, chatText.TrimEnd('\0'));
            }
        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    class ChatControllerUpdatePatch
    {
        public static int CurrentHistorySelection = -1;

        private static SpriteRenderer QuickChatIcon;
        private static SpriteRenderer OpenBanMenuIcon;
        private static SpriteRenderer OpenKeyboardIcon;

        public static void Prefix()
        {
            ModManager.Instance.ShowModStamp();//Shows the mod's stamp...Incase if you wanna remove it just delete this line.
        }

        public static void Postfix(ChatController __instance)
        {
            if (DarkModePlugin.DarkModeConfig.Value)
            {
                __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                __instance.freeChatField.textArea.compoText.Color(Color.white);
                __instance.freeChatField.textArea.outputText.color = Color.white;

                __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                __instance.quickChatField.text.color = Color.white;

                if (QuickChatIcon == null) QuickChatIcon = GameObject.Find("QuickChatIcon")?.transform.GetComponent<SpriteRenderer>();
                else QuickChatIcon.sprite = Modules.Utils.LoadSprite("DarkMode.ImageResource.DarkQuickChat.png", 100f);

                if (OpenBanMenuIcon == null) OpenBanMenuIcon = GameObject.Find("OpenBanMenuIcon")?.transform.GetComponent<SpriteRenderer>();
                else OpenBanMenuIcon.sprite = Modules.Utils.LoadSprite("DarkMode.ImageResource.DarkReport.png", 100f);

                if (OpenKeyboardIcon == null) OpenKeyboardIcon = GameObject.Find("OpenKeyboardIcon")?.transform.GetComponent<SpriteRenderer>();
                else OpenKeyboardIcon.sprite = Modules.Utils.LoadSprite("DarkMode.ImageResource.DarkKeyboard.png", 100f);
            }
            else
            {
                __instance.freeChatField.textArea.outputText.color = Color.black;
            }

            if (!__instance.freeChatField.textArea.hasFocus) return;
            __instance.freeChatField.textArea.characterLimit = AmongUsClient.Instance.AmHost ? 120 : 120;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
                ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
                __instance.freeChatField.textArea.SetText(__instance.freeChatField.textArea.text + GUIUtility.systemCopyBuffer);
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.X))
            {
                ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);
                __instance.freeChatField.textArea.SetText("");
            }
        }
    }
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public class PingTrackerPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            PingTracker pingTracker = GameObject.FindObjectOfType<PingTracker>();
            if (DarkModePlugin.ShowWatermark.Value &&  pingTracker != null)
            {
                pingTracker.text.text += "<br><size=2.3><#666>Dark Mode <sup><#3c39>[Dev]</sup></size>" + " <size=2><#f00>v1.1.5</size>" + " <size=1.5><color=#555>Made by<#39f> Techiee";
                //pingTracker.text.text += "<br><size=2.3><#666>Dark Mode <sup><#3c39>[Latest]</sup></size>" + " <size=2><#f00>v1.1.5</size>" + " <size=1.5><color=#555>Made by<#39f> Techiee";
                __instance.text.outlineColor = Color.black;
                __instance.text.alignment = TextAlignmentOptions.Center;
            }
        }
    }
}
