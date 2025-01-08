using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class CallAssignInList
    {
        public int? VolunteerId { get; set; }
        public string? VolunteerName { get; set; } = string.Empty;
        public DateTime EntranceTime { get; set; }
        public DateTime? FinishingTime { get; set; }
        public TypeOfFinishTreatment? TypeOfFinishTreatment {  get; set; }
    }
}
