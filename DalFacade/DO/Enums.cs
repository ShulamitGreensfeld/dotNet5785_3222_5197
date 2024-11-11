namespace DO;

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
}

public enum TypeOfFinishTreatment
{
  Treated,
  SelfCancellation,
  ManagerCancellation,
  OutOfRangeCancellation
}