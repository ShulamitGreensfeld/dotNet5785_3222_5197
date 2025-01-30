using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;


namespace BO
{
    public class CallInList
    {
        public int? Id { get; set; }
        public int CallId { get; set; }
        public TypeOfCall TypeOfCall {  get; set; }
        public DateTime OpeningTime { get; set; }
        public TimeSpan? TimeForClosing { get; set; }
        public string? LastVolunteerName { get; set; } 
        public TimeSpan? TimeForTreating {  get; set; }
        public CallStatus CallStatus { get; set; }
        public int TotalAssiignment {  get; set; }
        public override string ToString() => this.ToStringProperty();
    }
}
