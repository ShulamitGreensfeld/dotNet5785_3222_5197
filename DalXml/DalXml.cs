using DalApi;
namespace Dal;

sealed public class DalXml : IDal
{
    public ICall Call { get; } = new CallImplementation();
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();
    public IAssignment Assignment { get; } = new AssignmentImplementation();
    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Call.DeleteAll();
        Volunteer.DeleteAll();
        Assignment.DeleteAll();
        Config.Reset();
    }

}