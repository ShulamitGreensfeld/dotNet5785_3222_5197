namespace Dal;
using DalApi;
using DO;
using System.Linq;
using System.Runtime.CompilerServices; 
internal class AssignmentImplementation : IAssignment
{
    // This function returns all assignments from DataSource.
    // If a filter is provided, it returns only those assignments that match the filter.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null) //stage 2
    {
        return filter != null
            ? from item in DataSource.Assignments
              where filter(item)
              select item
            : from item in DataSource.Assignments
              select item;
    }

    // This function creates a new assignment with a new ID and adds it to DataSource.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Assignment item)
    {
        Assignment copy = item with { ID = Config.NextAssignmentId };
        DataSource.Assignments.Add(copy);
    }

    // This function deletes an assignment by its ID. If the assignment does not exist, it throws an exception.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        Assignment? newAssignment = Read(id);
        if (newAssignment == null)
            throw new DalDeletionImpossibleException($"Assignment with ID={id} does Not exist");
        else
            DataSource.Assignments.Remove(newAssignment);
    }

    // This function deletes all assignments from DataSource.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Assignments.Clear();
    }

    // This function searches for and returns an assignment by its ID.
    // If the assignment with the given ID is not found, it returns null.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(int id)
    {
        Assignment? newAssignment = DataSource.Assignments.FirstOrDefault(assignment => assignment!.ID == id); //stage 2
        return newAssignment; //stage2
    }

    // This function updates an existing assignment in DataSource.
    // If the assignment does not exist, it throws an exception.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Assignment item)
    {
        Assignment? newAssignment = Read(item.ID);
        if (newAssignment == null)
            throw new DalDoesNotExistException($"Assignment with ID={item.ID} does Not exist");
        else
        {
            DataSource.Assignments.Remove(newAssignment);
            DataSource.Assignments.Add(item);
        }
    }

    // This function searches for and returns an assignment based on a provided filter function.
    // If the filter is null, it throws an exception.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        if (filter == null)
            throw new NullException($"{nameof(filter)} Filter function cannot be null");

        return DataSource.Assignments.Cast<Assignment>().FirstOrDefault(filter);
    }
}
