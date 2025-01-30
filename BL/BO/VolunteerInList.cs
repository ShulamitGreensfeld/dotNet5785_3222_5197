using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;


namespace BO
{
    public class VolunteerInList
    {
        public int Id {  get; init; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalHandledCalls { get; set; }
        public int TotalCanceledCalls { get; set; }
        public int TotalOutOfRangeCall { get; set; }
        public int CallId {  get; set; }
        public TypeOfCall TypeOfCall { get; set; }
        public override string ToString() => this.ToStringProperty();
    }
}
