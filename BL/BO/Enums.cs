namespace BO;
public enum Role
{
    Manager,
    Volunteer
}
public enum DistanceType
{
    AirDistance,
    WalkingDistance,
    DrivingDistance
}
public enum TypeOfCall
{
    ToPrepareFood,
    ToCarryFood,
    ToPackageFood,
    ToDonateRawMaterials,
    ToCommunityCookingNights
}
public enum CallTritingByVulanteerStatus
{
    Treating,
    NextToRange
}
public enum TypeOfFinishTreatment
{
    Treated,
    SelfCancellation,
    ManagerCancellation,
    OutOfRangeCancellation
}
public enum CallStatus
{
    open,
    inTreat,
    closed,
    openInDanger,
    inTreatInDanger,
    OutOfRange
}

public enum TimeUnit 
{ 
    YEAR,
    MONTH,
    DAY,
    HOUR,
    MINUTE 
}
public enum VolunteerInListField
{
    Id,
    Name,
    TotalHandledCalls,
    TotalCanceledCalls,
    TotalOutOfRangeCall,
    CallId,
    TypeOfCall
}
public enum CallField
{
    Id,
    CallId,
    TypeOfCall,
    OpeningTime,
    TimeForClosing,
    LastVolunteerName,
    TimeForTreating,
    CallStatus,
    TotalAssiignment
}
