using System.Collections.Generic;

namespace SkisporetUpdates.Models
{
    public class TrackData
    {
        public int LineId { get; set; }
        public bool Skating { get; set; }
        public bool SnowScooterPreparation { get; set; }
        public bool Floodlit { get; set; }
        public long CalcTime { get; set; }
        public List<Status> Statuses { get; set; }
    }
}