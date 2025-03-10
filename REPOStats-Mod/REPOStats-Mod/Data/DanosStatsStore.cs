using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace REPOStats_Mod.Data
{
    //Class to store the stats of the player for serialization to the API
    [DataContract]
    public class DanosStatsStore
    {
        [DataMember]
        public long RoundStarted { get; set; } = 0;
        [DataMember]
        public long RoundEnded { get; set; } = 0;
        [DataMember]
        public string LevelName { get; set; } = "";

        [DataMember]
        public string LobbyHash { get; set; } = "";

        [DataMember]
        public string ModVersion { get; set; } = DanosRepoStatsPluginInfo.PLUGIN_VERSION;
        [DataMember]
        public string GameVersion { get; set; } = "Unknown";
        [DataMember] 
        public string RunEndReason { get; set; } = "";
        [DataMember]
        public long MySteamId { get; set; } = 0;
        [DataMember]
        public bool isHost { get; set; } = false;     
                
        [DataMember]
        public List<DanosDeathContainer> Deaths { get; set; } = new List<DanosDeathContainer>();

        [DataMember]
        public DanosRunStats RunStats { get; set; } = new();

    }
}
