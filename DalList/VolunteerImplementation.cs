namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

internal class VolunteerImplementation : IVolunteer
{
    /// <summary>
    /// Creates a new volunteer if it doesn't already exist.
    /// Throws an exception if a volunteer with the same ID already exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Volunteer item)
    {
        if (Read(item.ID) is not null)
            throw new DalAlreadyExistsException($"Volunteer with ID={item.ID} already exists");
        DataSource.Volunteers.Add(item);
    }

    /// <summary>
    /// Deletes a volunteer by their ID.
    /// Throws an exception if no volunteer with the given ID exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        Volunteer? newVolunteer = Read(id);
        if (newVolunteer == null)
            throw new DalDeletionImpossibleException($"Could not Update Item, no Volunteer with Id{id} found");
        else
            DataSource.Volunteers.Remove(newVolunteer);
    }

    /// <summary>
    /// Deletes all volunteers.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Volunteers.Clear();
    }

    /// <summary>
    /// Reads a volunteer by their ID.
    /// Returns the volunteer if found, or null if no volunteer with that ID exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(int id)
    {
        Volunteer? newvolunteer = DataSource.Volunteers.FirstOrDefault(volunteer => volunteer!.ID == id);
        return newvolunteer;
    }

    /// <summary>
    /// Reads all volunteers.
    /// Optionally filters the list of volunteers based on the provided filter function.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null)
        => filter != null
            ? from item in DataSource.Volunteers
              where filter(item)
              select item
            : from item in DataSource.Volunteers
              select item;

    /// <summary>
    /// Updates an existing volunteer's details.
    /// Throws an exception if no volunteer with the given ID exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
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

    /// <summary>
    /// Reads a volunteer based on a custom filter function.
    /// Throws an exception if the filter function is null.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        if (filter == null)
            throw new NullException($"{nameof(filter)} Filter function cannot be null");

        return DataSource.Volunteers.Cast<Volunteer>().FirstOrDefault(filter);
    }
}
