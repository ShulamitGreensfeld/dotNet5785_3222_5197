using Helpers;
using static BO.Enums;

namespace BO;

public class OpenCallInList
{
    public int Id { get; set; }
    public CallType CallType { get; set; }
    public string? Verbal_description { get; set; }
    public string FullAddress { get; set; }
    public DateTime Start_time { get; set; }
    public DateTime? Max_finish_time { get; set; }
    public double CallDistance { get; set; }
    public override string ToString() => this.ToStringProperty();
}