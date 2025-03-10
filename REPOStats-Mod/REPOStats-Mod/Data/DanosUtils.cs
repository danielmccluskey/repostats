using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace REPOStats_Mod.Data
{
    public static class DanosUtils
    {
        public static string GetHostSteamID()
        {
            try
            {
                var playerlist = SemiFunc.PlayerGetAll();
                if(playerlist == null || playerlist.Count == 0)
                {
                    return string.Empty;
                }
                foreach (var player in SemiFunc.PlayerGetAll())
                {
                    if (player != null && player.photonView != null && player.photonView.Owner.IsMasterClient)
                    {
                        return SemiFunc.PlayerGetSteamID(player);
                    }
                }

                return string.Empty;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static string GetMySteamID()
        {
            foreach (var player in SemiFunc.PlayerGetAll())
            {
                if (player != null && player.photonView.IsMine)
                {
                    return SemiFunc.PlayerGetSteamID(player);
                }
            }

            return string.Empty;
        }

        // Get a hash of the people in the lobby, so we can group lobbies, but not identify individual players that don't want to be identified
        public static string GetLobbyHash()
        {
            List<string> steamIds = new List<string>();
            string hostSteamID = GetHostSteamID();

            foreach (var player in SemiFunc.PlayerGetAll())
            {
                if (player != null)
                {
                    string steamID = SemiFunc.PlayerGetSteamID(player);
                    if (!string.IsNullOrEmpty(steamID) && steamID != hostSteamID)
                    {
                        steamIds.Add(steamID);
                    }
                }
            }

            if (!string.IsNullOrEmpty(hostSteamID))
            {
                steamIds.Insert(0, hostSteamID);
            }

            steamIds.Sort();
            string lobbyString = string.Join(",", steamIds);
            return ComputeSHA256(lobbyString);
        }

        private static string ComputeSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
