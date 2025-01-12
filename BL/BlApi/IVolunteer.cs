
namespace BlApi;

public interface IVolunteer
{
    BO.Role Login(string name, string password);
    IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive, BO.TypeOfCall? callType);
    BO.Volunteer GetVolunteerDetails(int volunteerId);
    void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteer);
    void DeleteVolunteer(int volunteerId);
    void AddVolunteer(BO.Volunteer volunteer);

    //IEnumerable<BO.VolunteerInList> GetAssignedCallsForVolunteer(int volunteerId, BO.Year year = BO.Year.None);
    //IEnumerable<BO.VolunteerInList> GetUnRegisteredCoursesForStudent(int studentId, BO.Year year = BO.Year.None);

    //BO.StudentGradeSheet GetGradeSheetPerStudent(int studentId, BO.Year year = BO.Year.None);
    //void UpdateGrade(int studentId, int courseId, double grade);
}
