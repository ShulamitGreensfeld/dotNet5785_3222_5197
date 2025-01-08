using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class OpenCallInList
    {
        public int Id { get; init; }
        public TypeOfCall TypeOfCall { get; set; }
        public string? CallDescription { get; set; } 
        public string Adress { get; set; } = string.Empty;
        public DateTime OpeningTime { get; set; }
        public DateTime? MaxTimeForClosing { get; set; }
        public double Distance { get; set; }
    }
}
