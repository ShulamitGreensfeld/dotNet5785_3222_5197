namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

internal class VolunteerImplementation : IVolunteer
{
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null) //stage 2
        => filter != null
            ? from item in DataSource.Volunteers
              where filter(item)
              select item
            : from item in DataSource.Volunteers
              select item;

    //public void Create(Volunteer item)
    //{
    //    Volunteer? v = Read(item.ID);
    //    if (v is not null)
    //        throw new Exception($"Volunteer Object with {item.ID} already exists");
    //    else
    //        DataSource.Volunteers.Add(item);
    //}
    public void Create(Volunteer item)
    {
        //for entities with normal id (not auto id)
        if (Read(item.ID) is not null)
            throw new DalAlreadyExistsException($"Volunteer with ID={item.ID} already exists");
        DataSource.Volunteers.Add(item);
    }

    public void Delete(int id)
    {
        Volunteer? newVolunteer = Read(id);
        if (newVolunteer == null)
            throw new DalDeletionImpossibleException($"Could not Update Item, no Volunteer with Id{id} found");
        else
            DataSource.Volunteers.Remove(newVolunteer);
    }

    public void DeleteAll()
    {
        DataSource.Volunteers.Clear();
    }

    public Volunteer? Read(int id)
    {
        //Volunteer? newvolunteer = DataSource.Volunteers.Find(volunteer => volunteer!.ID == id); //stage1
        //return newvolunteer; //stage1
        Volunteer? newvolunteer = DataSource.Volunteers.FirstOrDefault(volunteer => volunteer!.ID == id); //stage 2
        return newvolunteer; //stage2
    }

    //public List<Volunteer> ReadAll() //stage1
    //{
    //    return new List<Volunteer>(DataSource.Volunteers!);
    //}

    public void Update(Volunteer item)
    {
        Volunteer? newVolunteer = Read(item.ID);
        if (newVolunteer == null)
            throw new DalDoesNotExistException($"Could not Update Item, no Volunteer with Id{item.ID} found");
        else
        {
            DataSource.Volunteers.Remove(newVolunteer);
            DataSource.Volunteers.Add(item);
        }
    }
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        if (filter == null)
            throw new NullException($"{nameof(filter)} Filter function cannot be null");

        return DataSource.Volunteers.Cast<Volunteer>().FirstOrDefault(filter);
    }
}