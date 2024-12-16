namespace DalApi;

public interface IDal
{
    ICall Call { get; }
    IVolunteer Volunteer { get; }
    IAssignment Assignment { get; }

    IConfig Config { get; }
    void ResetDB();
}

