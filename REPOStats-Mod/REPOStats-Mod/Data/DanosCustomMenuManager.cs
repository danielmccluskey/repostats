﻿using MenuLib;
using MenuLib.MonoBehaviors;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace REPOStats_Mod.Data
{
    internal class DanosCustomMenuManager : MonoBehaviour
    {
        private static REPOPopupPage challengesPage;
        private static List<REPOButton> challengeButtons = new List<REPOButton>();
        private const string ApiUrl = "https://repo-api.splitstats.io/api/challenges";
        private const string DiscordInviteUrl = "https://discord.gg/vPJtKhYAFe"; // Replace with actual Discord link

        public static void Initialize()
        {
            MenuAPI.AddElementToEscapeMenu(parent =>
            {
            var statsbutton = MenuAPI.CreateREPOButton("REPOStats Challenges", () => CreateChallengesPage().OpenPage(false), parent, new Vector2(356f, 86f));

        });
                
                
        }

        private static REPOPopupPage CreateChallengesPage()
        {
            challengesPage = null;
            challengeButtons.Clear();

            challengesPage = MenuAPI.CreateREPOPopupPage("REPOStats Challenges", true, 0.0f);

            challengesPage.AddElement(parent =>
            {
                var backButton = MenuAPI.CreateREPOButton("Back", () => challengesPage.ClosePage(true), parent, new Vector2(77f, 34f));

                // Add placeholder buttons before fetching
                for (int i = 0; i < 5; i++) // Default to 5 placeholders
                {
                    var button = MenuAPI.CreateREPOButton("Loading...", null, parent, new Vector2(100f, 250f - (i * 50f)));
                    challengeButtons.Add(button);
                }


            });




            // Start fetching challenges dynamically
            var fetcher = new GameObject("ChallengeFetcher").AddComponent<ChallengeFetcher>();
            fetcher.StartFetching();

            return challengesPage;
        }

        public static void UpdateChallengeButtons(List<string> challenges)
        {
            float startY = 250f;
            float spacing = 50f;

            // Ensure there are enough buttons or add more
            while (challengeButtons.Count < challenges.Count)
            {

                challengesPage.AddElement(parent =>
                {
                    var button = MenuAPI.CreateREPOButton("Loading...", null, parent, new Vector2(250f, startY - (challengeButtons.Count * spacing)));
                    challengeButtons.Add(button);
                });

            }

            for (int i = 0; i < challengeButtons.Count; i++)
            {
                if (i < challenges.Count)
                {
                    string challengeText = challenges[i];
                    if (challengeText == "Click here to join!")
                    {

                        challengeButtons[i].button.onClick.AddListener(() => Application.OpenURL(DiscordInviteUrl));
                        challengeButtons[i].labelTMP.text = challengeText;


                    }
                    else
                    {
                        challengeButtons[i].labelTMP.text = challengeText;
                    }
                }
                else
                {
                    challengeButtons[i].labelTMP.text = "";
                }
            }
        }
    }

    public class ChallengeFetcher : MonoBehaviour
    {
        private const string ApiUrl = "aHR0cHM6Ly9yZXBvLWFwaS5zcGxpdHN0YXRzLmlvL2FwaS9jaGFsbGVuZ2Vz";
        private long mySteamId;
        public void StartFetching()
        {
            mySteamId = 0;
            string mySteamIdStr = DanosUtils.GetMySteamID();
            if (string.IsNullOrEmpty(mySteamIdStr))
            {
                Debug.LogError("Could not get my steam id");
            }
            //try parse as long
            if (!long.TryParse(mySteamIdStr, out mySteamId))
            {
                Debug.LogError("Could not parse steam id as long");
            }
            StartCoroutine(FetchChallenges());
        }

        private IEnumerator FetchChallenges()
        {
            //Base64 decode the API URL
            string apiUrl = Encoding.UTF8.GetString(Convert.FromBase64String(ApiUrl));
            string ApiUrlWSteam = $"{apiUrl}?steamid={mySteamId}";
            using (UnityWebRequest request = UnityWebRequest.Get(ApiUrlWSteam))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to fetch challenges: {request.error}");
                    DanosCustomMenuManager.UpdateChallengeButtons(new List<string> { "Error loading challenges." });
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    List<string> challenges = ParseChallenges(jsonResponse);
                    DanosCustomMenuManager.UpdateChallengeButtons(challenges);
                }
            }
            Destroy(gameObject); // Cleanup fetcher after fetching
        }

        private List<string> ParseChallenges(string jsonResponse)
        {
            try
            {
                return JsonUtility.FromJson<ChallengeList>("{\"challenges\": " + jsonResponse + "}").challenges;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing challenge data: {ex.Message}");
                return new List<string> { "Failed to load challenges." };
            }
        }

        [Serializable]
        private class ChallengeList
        {
            public List<string> challenges;
        }
    }
}
