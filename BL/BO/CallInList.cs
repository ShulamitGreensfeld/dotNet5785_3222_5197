using Helpers;
using Microsoft.VisualBasic;
using static BO.Enums;
namespace BO;

public class CallInList
{
    public int? Id { get; set; }
    public int CallId { get; set; }
    public Enums.CallType CallType { get; set; }
    public DateTime Opening_time { get; set; }
    public TimeSpan? TimeLeft { get; set; }
    public string? LastVolunteerName { get; set; }
    public TimeSpan? TotalTime { get; set; }
    public CallStatus CallStatus { get; set; }
    public int TotalAssignments { get; set; }
    public override string ToString() => this.ToStringProperty();
}
