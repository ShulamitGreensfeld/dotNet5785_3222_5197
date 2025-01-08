using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class VolunteerInList
    {
        public int Id {  get; init; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TreatedCallNum { get; set; }
        public int CenteledCallNum { get; set; }
        public int OutOfRangeCallNum { get; set; }
        public int CallId {  get; set; }
        public TypeOfCall TypeOfCall { get; set; }
    }
}
