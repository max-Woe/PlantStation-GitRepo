using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class ReceivedMeasurement
    {
        public DateTime Time { get; private set; }

        // Der Deserializer verwendet diese Eigenschaft, um den Unix-Timestamp zu setzen.
        public long UnixTime
        {
            set => Time = DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
        }
        public float Value { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
        public int Pin { get; set; }
        public string MacAddress { get; set; }
    }
}
