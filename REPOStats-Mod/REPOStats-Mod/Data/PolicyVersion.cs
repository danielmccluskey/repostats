using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace REPOStats_Mod.Data
{
    
    [DataContract]
    public class PolicyVersion
    {
        [DataMember(Name = "version")]
        public string Version { get; set; } = "0.0.0";

        [DataMember(Name = "lastUpdated")]
        public string LastUpdatedRaw { get; set; } = "2025-03-06";

        [DataMember(Name = "descriptionOfChanges")]
        public string DescriptionOfChanges { get; set; } = "Newly drafted policy to clear some things up.";

        public DateTime LastUpdated
        {
            get
            {
                if (DateTime.TryParseExact(LastUpdatedRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
                return DateTime.MinValue; // Default fallback
            }
        }

        public string LastUpdatedDaysAgo => $"{(DateTime.UtcNow - LastUpdated).Days} day(s) ago";
    }
}
