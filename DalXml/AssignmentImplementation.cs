namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

internal class AssignmentImplementation : IAssignment
{
    // Load all assignments from the XML file, optionally filtering them
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)//stage 3
    {
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return filter != null ? assignments.Where(filter) : assignments;
    }

    // Create a new assignment and save it to the XML file
    public void Create(Assignment item)//stage 3
    {
        // Assign a new unique ID to the assignment
        Assignment newAssignment = item with { ID = Config.NextAssignmentId };

        // Load existing assignments, add the new one, and save the updated list
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        assignments.Add(newAssignment);
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);

        //Console.WriteLine($"Assignment with ID {newAssignment.ID} created successfully.");
    }

    // Read a specific assignment by its ID
    public Assignment? Read(int id)//stage 3
    {
        Console.WriteLine($"Attempting to read assignment with ID: {id}");

        // Load assignments and find the one matching the ID
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        Assignment? assignment = assignments.FirstOrDefault(a => a.ID == id);

        if (assignment == null)
            Console.WriteLine($"Assignment with ID {id} not found.");

        return assignment;
    }

    // Delete an assignment by its ID
    public void Delete(int id)//stage 3
    {
        // Load assignments and find the one to delete
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        Assignment? assignmentToDelete = assignments.FirstOrDefault(a => a.ID == id);

        if (assignmentToDelete == null)
            throw new DalDeletionImpossibleException($"Assignment with ID {id} does not exist.");

        // Remove the assignment and save the updated list
        assignments.Remove(assignmentToDelete);
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);

        //Console.WriteLine($"Assignment with ID {id} deleted successfully.");
    }

    // Delete all assignments from the XML file
    public void DeleteAll()//stage 3
    {
        XMLTools.SaveListToXMLSerializer(new List<Assignment>(), Config.s_assignments_xml);
        //Console.WriteLine("All assignments deleted successfully.");
    }

    // Update an existing assignment
    public void Update(Assignment item)//stage 3
    {
        // Load assignments and find the one to update
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        int index = assignments.FindIndex(a => a.ID == item.ID);

        if (index == -1)
            throw new DalDoesNotExistException($"Assignment with ID {item.ID} does not exist.");

        // Update the assignment and save the updated list
        assignments[index] = item;
        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);

        //Console.WriteLine($"Assignment with ID {item.ID} updated successfully.");
    }

    // Read a specific assignment using a filter function
    public Assignment? Read(Func<Assignment, bool> filter)//stage 3
    {
        if (filter == null)
            throw new NullException("Filter function cannot be null.");

        // Load assignments and find the first matching one
        List<Assignment> assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        return assignments.FirstOrDefault(filter);
    }
}
