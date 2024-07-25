using AmongUs.QuickChat;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using Rewired.Utils.Platforms.Windows;
using AmongUs.Data;
using InnerNet;

[BepInPlugin("com.darkmode.techiee", "DarkMode", "1.0")]
public class Plugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony("Dark Mode, by Techiee.");
    public static ConfigEntry<bool> DarkModeConfig;

    public override void Load()
    {
        DarkModeConfig = Config.Bind("DarkMode",
                                     "DarkMode",
                                     true,
                                     "Set this to false if you don't want dark mode for now");
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
            if (Plugin.DarkModeConfig.Value) sr.color = new Color(0, 0, 0, 128);

            if (chatText.Contains("░") ||
                chatText.Contains("▄") ||
                chatText.Contains("█") ||
                chatText.Contains("▌") ||
                chatText.Contains("▒")) ;
            else
            {
                if (Plugin.DarkModeConfig.Value) chatText = ColorString(Color.white, chatText.TrimEnd('\0'));
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
            if (AmongUsClient.Instance.AmHost && DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly)
                DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }

        public static void Postfix(ChatController __instance)
        {
            if (Plugin.DarkModeConfig.Value)
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
            __instance.freeChatField.textArea.characterLimit = AmongUsClient.Instance.AmHost ? 2000 : 300;

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
        public static void Postfix()
        {
            PingTracker pingTracker = GameObject.FindObjectOfType<PingTracker>();
            if (pingTracker != null)
            {
                pingTracker.text.text += "<br><size=2.3><#666>Dark Mode <sup><#3c39>[Dev]</sup></size>" + " <size=2><#f00>v1.0</size>" + " <size=2><color=#555>Made by<#39f> 〒∈⊂卄∥∈∈";
            }
        }
    }
}
