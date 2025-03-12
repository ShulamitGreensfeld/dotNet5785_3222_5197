using Helpers;
using static BO.Enums;
namespace BO;

public class CallAssignInList
{
    public int? VolunteerId { get; set; }
    public string? VolunteerFullName { get; set; }
    public DateTime Start_time { get; set; }
    public DateTime? End_time { get; set; }
    public EndType? EndType { get; set; }
    public override string ToString() => this.ToStringProperty();
}