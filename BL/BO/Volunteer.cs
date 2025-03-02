using Helpers;
using static BO.Enums;
namespace BO;

public class Volunteer
{
    public int Id { get; init; }
    public string FullName { get; set; }
    public string CellphoneNumber { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }/////////////////////////לשים לב לתוספת
    public string? FullAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Role Role { get; set; }
    public bool IsActive { get; set; }
    public DistanceTypes DistanceType { get; set; }
    public double? MaxDistance { get; set; }
    public int TotalHandledCalls { get; set; }
    public int TotalCanceledCalls { get; set; }
    public int TotalExpiredCalls { get; set; }
    public BO.CallInProgress? CallInProgress { get; set; }
    public override string ToString() => this.ToStringProperty();
}