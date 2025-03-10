using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace REPOStats_Mod.Data
{
    [DataContract]
    public class DanosRunStats
    {
        [DataMember]
        public int extractions_completed { get; set; } = 0;
        [DataMember]
        public int extractions_on_map { get; set; } = 0;
        [DataMember]
        public bool failed { get; set; } = false;
        [DataMember]
        public int top_extraction_target { get; set; } = 0;
        [DataMember]
        public int take_home_money { get; set; } = 0;
        [DataMember]
        public int total_money { get; set; } = 0;
        [DataMember]
        public int completed_levels { get; set; } = 0;

        [DataMember]
        public string extraction_goals_csv { get; set; } = "";
    }
}
