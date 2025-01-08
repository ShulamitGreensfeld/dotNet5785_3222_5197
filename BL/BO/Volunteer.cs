using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class Volunteer
    {
        public int Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public  string Phone {  get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Adress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Role Role { get; set; }
        public bool IsActive {  get; set; }
        public double? MaxDistanceForCall {  get; set; }
        public DistanceType DistanceType { get; set; }
        public int TreatedCallNum {  get; set; }
        public int CenteledCallNum { get; set; }
        public int OutOfRangeCallNum { get; set; }
        public BO.CallInProgress? CallInProgress {  get; set; }
    }
}
