namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

internal class AssignmentImplementation : IAssignment
{
    // Load all assignments from the XML file, optionally filtering them
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return filter != null ? assignments.Where(filter) : assignments;
    }

    // Create a new assignment and save it to the XML file
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Assignment item)
    {
        Assignment newAssignment = item with { ID = Config.NextAssignmentId };

        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        assignments.Add(newAssignment);
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }

    // Read a specific assignment by its ID
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(int id)
    {
        Console.WriteLine($"Attempting to read assignment with ID: {id}");

        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        Assignment? assignment = assignments.FirstOrDefault(a => a.ID == id);

        if (assignment == null)
            Console.WriteLine($"Assignment with ID {id} not found.");

        return assignment;
    }

    // Delete an assignment by its ID
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        Assignment? assignmentToDelete = assignments.FirstOrDefault(a => a.ID == id);

        if (assignmentToDelete == null)
            throw new DalDeletionImpossibleException($"Assignment with ID {id} does not exist.");

        assignments.Remove(assignmentToDelete);
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }

    // Delete all assignments from the XML file
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Assignment>(), Config.s_assignments_xml);
    }

    // Update an existing assignment
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Assignment item)
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        int index = assignments.FindIndex(a => a.ID == item.ID);

        if (index == -1)
            throw new DalDoesNotExistException($"Assignment with ID {item.ID} does not exist.");

        assignments[index] = item;
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
    }

    // Read a specific assignment using a filter function
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        if (filter == null)
            throw new NullException("Filter function cannot be null.");

        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return assignments.FirstOrDefault(filter);
    }
}
