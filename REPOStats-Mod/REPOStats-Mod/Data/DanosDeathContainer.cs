using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace REPOStats_Mod.Data
{
    [DataContract]
    public class DanosDeathContainer
    {
        [DataMember]
        public string CauseOfDeath { get; set; } = "";
        [DataMember]
        public long DeathTime { get; set; }
    }
}
