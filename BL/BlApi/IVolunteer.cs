//namespace BlApi;

//public interface IVolunteer
//{
//    BO.Role Login(string name, string password);
//    void Create(BO.Volunteer boVolunteer);
//    IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive, BO.TypeOfCall? callType);
//    BO.Volunteer GetVolunteerDetails(int volunteerId);
//    void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteer);
//    void DeleteVolunteer(int volunteerId);
//    void AddVolunteer(BO.Volunteer volunteer);
//}
using System;
using System.Collections.Generic;
using BO;

namespace BlApi
{
    public interface IVolunteer
    {
        Role Login(string name, string password);
        void Create(Volunteer boVolunteer);
        IEnumerable<Volunteer> GetVolunteersList(bool? isActive, TypeOfCall? callType);
        Volunteer GetVolunteerDetails(int volunteerId);
        void UpdateVolunteerDetails(int requesterId, Volunteer volunteer);
        void DeleteVolunteer(int volunteerId);
        void AddVolunteer(Volunteer volunteer);
    }
}