using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkisporetUpdates.Models
{
    class TrackStatus
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int PrepStatus { get; set; }
    }
}
