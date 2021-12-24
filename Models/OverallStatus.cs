using System;

namespace SkisporetUpdates.Models
{
    public class TrackStatus
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int PrepStatus { get; set; }
        public string MailStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
