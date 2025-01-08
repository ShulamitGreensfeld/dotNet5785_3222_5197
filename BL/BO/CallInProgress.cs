using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class CallInProgress
    {
        public int Id { get; init; }
        public int CallId { get; init; }
        public TypeOfCall TypeOfCall { get; set; }
        public string? CallDescription { get; set; }
        public string Adress { get; set; } = string.Empty;
        public DateTime OpeningTime { get; set; }
        public DateTime? MaxTimeForClosing { get; set; }
        public DateTime EntraceTime { get; set; }
        public double Distance { get; set; }
        public CallTritingByVulanteerStatus CallTritingByVulanteerStatus {  get; set; }
    }
}
