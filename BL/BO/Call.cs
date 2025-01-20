using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public  class Call
    {
        public int Id {  get; set; }
        public TypeOfCall TypeOfCall { get; set; }
        public string? CallDescription { get; set; }
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime OpeningTime { get; set; }
        public DateTime? MaxTimeForClosing { get; set; }
        public CallStatus CallStatus { get; set; }
        public List<BO.CallAssignInList>? AssignmentList { get; set; } = null;
    }
}
