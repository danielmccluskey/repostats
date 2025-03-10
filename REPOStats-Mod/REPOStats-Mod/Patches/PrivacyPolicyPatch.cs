using HarmonyLib;
using REPOStats_Mod.Data;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Json;
namespace REPOStats_Mod.Patches
{

    [HarmonyPatch]
    public class PrivacyPolicyPatch
    {
        public static PolicyVersion policy = new PolicyVersion();
        public static bool running = false;

        [HarmonyPatch(typeof(MenuManager), "Start")]
        [HarmonyPrefix]
        public static bool StartPrefix()
        {
            Debug.Log("Checking if user has accepted the current policy version.");

            // Check if the policy has already been loaded
            if (UnityEngine.Object.FindObjectOfType<PolicyLoader>() != null)
            {
                return true;
            }

            // Start a coroutine on Unity's main thread
            new GameObject("PolicyLoader").AddComponent<PolicyLoader>();



            return true;
        }
        public class PolicyLoader : MonoBehaviour
        {
            private async void Start()
            {
                await LoadPolicyAndShowPopup();
                Destroy(gameObject); // Cleanup after finishing
            }

            private async Task LoadPolicyAndShowPopup()
            {
                policy = await GetPolicy();
                Debug.Log("Policy version: " + policy.Version);

                if (HasUserAcceptedPolicy(policy.Version))
                {
                    Debug.Log("User has already accepted the current policy version.");
                    REPOStats_Mod.ApplyAdditionalPatches();

                    return;
                }

                Debug.Log("User has not accepted the current policy version. Showing the popup.");
                ShowPopup(
                    () =>
                    {
                        Debug.Log("User accepted REPOStats terms.");
                        SaveUserResponse(true, policy.Version);
                        REPOStats_Mod.ApplyAdditionalPatches();

                    },
                    () =>
                    {
                        Debug.Log("User declined REPOStats terms. Closing the game.");
                        Application.Quit();
                    }
                );
            }
        }
        private static bool HasUserAcceptedPolicy(string policyVersion)
        {
            try
            {
                // Get the directory of the currently executing assembly
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string directory = System.IO.Path.GetDirectoryName(assemblyLocation);
                string filePath = System.IO.Path.Combine(directory, "REPOStatsPrivacyResponse.txt");

                // Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return false; // User has not accepted any policy
                }

                // Read the file content
                string[] lines = System.IO.File.ReadAllLines(filePath);

                // Check if the file contains the current policy version
                foreach (var line in lines)
                {
                    if (line.Contains($"Policy Version: {policyVersion}"))
                    {
                        return true; // User has accepted the current policy version
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error checking user response: " + e.Message);
            }

            return false; // Default to false if any error occurs
        }
        private static void SaveUserResponse(bool accepted, string policyVersion)
        {
            try
            {
                // Get the directory of the currently executing assembly (where the mod is installed)
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string directory = System.IO.Path.GetDirectoryName(assemblyLocation);
                string filePath = System.IO.Path.Combine(directory, "REPOStatsPrivacyResponse.txt");

                // Write the response and policy version to the file
                string response = accepted ? "Accepted" : "Declined";
                string content = $"User Response: {response}\nPolicy Version: {policyVersion}\nDate: {DateTime.Now}\n\nDelete this file to revoke RepoStats Privacy policy consent for this mod profile. \nYou will need to do this for each mod profile that has RepoStats installed if you are using a mod manager like r2modman. ";
                System.IO.File.WriteAllText(filePath, content);
                Debug.Log("User response saved to: " + filePath);
            }
            catch (Exception e)
            {
                Debug.Log("Error saving user response: " + e.Message);
            }
        }
        [HarmonyPatch(typeof(MenuManager), "Update")]
        [HarmonyPrefix]
        public static void UpdatePostfix()
        {
            GlobalInputManager.Update();

        }
        public static void ShowPopup(Action onAccept, Action onDecline)
        {
            Debug.Log("Creating fullscreen popup...");

            var popupCanvas = new GameObject("RepoStatsPopupCanvas");
            var canvas = popupCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;  // Ensure it is rendered on top

            var canvasScaler = popupCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

            popupCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var panel = new GameObject("Panel");
            panel.transform.SetParent(popupCanvas.transform, false);
            var panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0, 0, 0, 0.95f); // Black transparent background
            var panelRectTransform = panel.GetComponent<RectTransform>();
            panelRectTransform.anchorMin = Vector2.zero;
            panelRectTransform.anchorMax = Vector2.one;
            panelRectTransform.sizeDelta = Vector2.zero;

            // Add Header
            CreateText(panel, "REPOStats", new Vector2(0, 150), 36, TextAnchor.UpperCenter, "#6ac4a7");

            // Add Explanation Text
            string message = "Welcome to REPOStats! By using this mod, you agree to the privacy policy at https://repo.splitstats.io/privacypolicy.\n\n"
                           + "Game stats will be collected and uploaded during gameplay. Your response to the policy will be saved in the mod folder until you uninstall the mod or the policy changes.\n\n"
                           + "Manage or delete your data using tools on our website. Decline to close the game and uninstall the mod.";

            // Display policy details
            string message2 = $"We do not store contact information, so we cannot directly inform you of any changes, however this prompt will popup any time we make any.\n\nVersion: {policy.Version}\nLast Updated: {policy.LastUpdated.ToShortDateString()}\nChanges: {policy.DescriptionOfChanges}\nLast Updated: {policy.LastUpdatedDaysAgo}.";

            CreateText(panel, message, new Vector2(0, 75), 8, TextAnchor.MiddleCenter, "#FFFFFF");
            CreateText(panel, message2, new Vector2(0, -25), 5, TextAnchor.MiddleCenter, "#FFFFFF");

            CreateText(panel, "[F5] Open Privacy Policy", new Vector2(0, -60), 24, TextAnchor.MiddleCenter, "#6ac4a7");
            CreateText(panel, "[F6] Accept", new Vector2(-100, -120), 24, TextAnchor.MiddleCenter, "#6ac4a7");
            CreateText(panel, "[F7] Decline", new Vector2(100, -120), 24, TextAnchor.MiddleCenter, "#FF6A6A");

            //Listen for f5 key
            GlobalInputManager.ListenForKeys(KeyCode.F5, () =>
            {
                Application.OpenURL("https://repo.splitstats.io/privacypolicy");

            });

            // Monitor keyboard inputs globally
            GlobalInputManager.ListenForKeys(KeyCode.F6, () =>
            {
                UnityEngine.Object.Destroy(popupCanvas);
                GlobalInputManager.StopListeningForKeys(KeyCode.F5);

                GlobalInputManager.StopListeningForKeys(KeyCode.F6);
                GlobalInputManager.StopListeningForKeys(KeyCode.F7);
                onAccept?.Invoke();
            });

            GlobalInputManager.ListenForKeys(KeyCode.F7, () =>
            {
                UnityEngine.Object.Destroy(popupCanvas);
                GlobalInputManager.StopListeningForKeys(KeyCode.F5);

                GlobalInputManager.StopListeningForKeys(KeyCode.F6);
                GlobalInputManager.StopListeningForKeys(KeyCode.F7);
                onDecline?.Invoke();
            });
        }



        private static async Task<PolicyVersion> GetPolicy()
        {
            var policy = new PolicyVersion();
            using (HttpClient client = new HttpClient())
            {
                Debug.Log("Getting policy from: https://repo.splitstats.io/api/Privacy/current");

                HttpResponseMessage response = await client.GetAsync("https://repo.splitstats.io/api/Privacy/current");

                Debug.Log("Response: " + response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    policy = DeserializeJson<PolicyVersion>(json);
                }
            }

            return policy;
        }


        private static T DeserializeJson<T>(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        private static void CreateText(GameObject parent, string message, Vector2 position, int fontSize = 20, TextAnchor alignment = TextAnchor.MiddleCenter, string hexColor = "#FFFFFF", Color? backgroundColor = null)
        {
            var textObj = new GameObject("TextElement");
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = HexToColor(hexColor);

            var textRectTransform = text.GetComponent<RectTransform>();
            textRectTransform.sizeDelta = new Vector2(460, 100);
            textRectTransform.anchoredPosition = position;
            textRectTransform.SetParent(parent.transform, false);

            if (backgroundColor.HasValue)
            {
                var bgObj = new GameObject("Background");
                var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
                bgImage.color = backgroundColor.Value;

                var bgRectTransform = bgObj.GetComponent<RectTransform>();
                bgRectTransform.sizeDelta = new Vector2(220, 60); // Size of the button background
                bgRectTransform.anchoredPosition = position;
                bgRectTransform.SetParent(parent.transform, false);

                text.transform.SetParent(bgObj.transform, false);
            }
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
            {
                return color;
            }
            return Color.white; // Default to white if parsing fails
        }


    }

    public static class GlobalInputManager
    {
        private static readonly Dictionary<KeyCode, Action> keyActions = new();

        public static void ListenForKeys(KeyCode key, Action action)
        {
            if (!keyActions.ContainsKey(key))
            {
                keyActions[key] = action;
            }
        }

        public static void StopListeningForKeys(KeyCode key)
        {
            if (keyActions.ContainsKey(key))
            {
                keyActions.Remove(key);
            }
        }

        public static void StopAllListening()
        {
            keyActions.Clear(); // Remove all listeners
        }

        public static void Update()
        {



            if (keyActions.Count == 0)
            {
                return;
            }
            foreach (var keyAction in keyActions)
            {
                if (Input.GetKeyDown(keyAction.Key))
                {
                    keyAction.Value?.Invoke();
                }
            }
        }
    }

}
