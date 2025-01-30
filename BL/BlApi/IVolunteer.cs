using System;
using System.Collections.Generic;

namespace BlApi
{
    public interface IVolunteer
    {
        BO.Role Login(string name, string password);
        IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive, BO.VolunteerInListField? sortBy, BO.TypeOfCall? callTypeFilter);
        BO.Volunteer GetVolunteerDetails(int volunteerId);
        void UpdateVolunteerDetails(int Id, BO.Volunteer volunteer);
        void DeleteVolunteer(int volunteerId);
        void AddVolunteer(BO.Volunteer volunteer);
    }
}

