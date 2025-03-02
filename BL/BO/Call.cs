using Helpers;
using Microsoft.VisualBasic;
using static BO.Enums;
namespace BO;

public class Call
{
    public int Id { get; set; }
    public Enums.CallType CallType { get; set; }
    public string? Verbal_description { get; set; }
    public string? FullAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime Opening_time { get; set; }
    public DateTime Max_finish_time { get; set; }
    public CallStatus CallStatus { get; set; }
    public List<BO.CallAssignInList>? AssignmentsList { get; set; }
    public override string ToString() => this.ToStringProperty();


}