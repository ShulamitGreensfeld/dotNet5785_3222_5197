using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;


namespace BO
{
    public class ClosedCallInList
    {
        public int Id { get; init; }
        public TypeOfCall TypeOfCall { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime OpeningTime { get; set; }
        public DateTime EntraceTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public TypeOfFinishTreatment? TypeOfFinishTreatment {  get; set; }
        public override string ToString() => this.ToStringProperty();
    }
}
