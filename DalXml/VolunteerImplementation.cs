namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

internal class VolunteerImplementation : IVolunteer
{
    // Converts an XElement representing a volunteer to a Volunteer object.

    static Volunteer getVolunteer(XElement v)
    {
        // Extract and parse the volunteer's ID.
        return new DO.Volunteer()
        {
            ID = v.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            Name = (string?)v.Element("FirstName") ?? "",
            Phone = (string?)v.Element("Phone") ?? "",
            Email = (string?)v.Element("Email") ?? "",
            IsActive = (bool?)v.Element("IsActive") ?? false,
            Role = Enum.TryParse<Role>((string)v.Element("Role")! ?? "", out var role) ? role : Role.Volunteer,
            Password = (string?)v.Element("Password") ?? "",
            Address = (string?)v.Element("Address") ?? "",
            Latitude = v.ToDoubleNullable("Latitude") ?? throw new FormatException("can't convert Latitude"),
            Longitude = v.ToDoubleNullable("Longitude") ?? throw new FormatException("can't convert Longitude"),
            MaxDistanceForCall = v.ToDoubleNullable("LargestDistance") ?? throw new FormatException("can't convert LargestDistance"),
            DistanceType = Enum.TryParse<DistanceType>((string)v.Element("DistanceType")! ?? "", out var distance) ? distance : DistanceType.WalkingDistance,
        };
    }
    // Creates a new volunteer element in the XML file.
    public void Create(Volunteer item)
    {
        XElement volunteerElement = CreateVolunteerElement(item);
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        volunteersRootElem.Add(volunteerElement);
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }
    // Helper method to convert a Volunteer object into an XElement.
    private XElement CreateVolunteerElement(Volunteer item)
    {
        return new XElement("Volunteer",
            new XElement("Id", item.ID),
            new XElement("Name", item.Name),
            new XElement("Phone", item.Phone),
            new XElement("Email", item.Email),
            new XElement("IsActive", item.IsActive),
            new XElement("Role", item.Role.ToString()),
            new XElement("Password", item.Password),
            new XElement("Address", item.Address),
            new XElement("Latitude", item.Latitude),
            new XElement("Longitude", item.Longitude),
            new XElement("LargestDistance", item.MaxDistanceForCall),
            new XElement("DistanceType", item.DistanceType.ToString())
        );
    }
    // Deletes a volunteer by their ID from the XML file.
    public void Delete(int id)
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        XElement volunteerElem = volunteersRootElem.Elements("Volunteer")
            .FirstOrDefault(v => (int)v.Element("Id")! == id)!;
        if ((volunteerElem) == null)
        {
            throw new DalDoesNotExistException($"Volunteer with ID={id} not found.");
        }
        volunteerElem.Remove();
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }

    // Deletes all volunteers from the XML file.

    public void DeleteAll()
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        volunteersRootElem.RemoveAll();
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }
    // Reads a volunteer by their ID from the XML file.
    public Volunteer? Read(int id)
    {
        XElement? volunteerElem =
        XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return volunteerElem is null ? null : getVolunteer(volunteerElem);
    }
    // Reads a single volunteer that matches the given filter.
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().Select(v => getVolunteer(v)).FirstOrDefault(filter);
    }
    // Reads all volunteers from the XML file, optionally filtering them.
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null)
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        var volunteers = volunteersRootElem.Elements("Volunteer")
            .Select(v => new Volunteer
            {
                ID = (int)v.Element("Id")!,
                Name = (string)v.Element("Name")!,
                Phone = (string)v.Element("Phone")!,
                Email = (string)v.Element("Email")!,
                IsActive = (bool)v.Element("IsActive")!,
                Role = (Role)Enum.Parse(typeof(Role), (string)v.Element("Role")!),
                Password = (string)v.Element("Password")!,
                Address = (string)v.Element("Address")!,
                Latitude = (double)v.Element("Latitude")!,
                Longitude = (double)v.Element("Longitude")!,
                MaxDistanceForCall = (double)v.Element("LargestDistance")!,
                DistanceType = (DistanceType)Enum.Parse(typeof(DistanceType), (string)v.Element("DistanceType")!)
            })
            .ToList();
        return filter == null ? volunteers : volunteers.Where(filter);
    }
    // Updates an existing volunteer in the XML file with new information.
    public void Update(Volunteer item)
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);

        (volunteersRootElem.Elements().FirstOrDefault(v => (int?)v.Element("Id") == item.ID)
        ?? throw new DO.DalDoesNotExistException($"Volunteer with ID={item.ID} does Not exist"))
                .Remove();

        volunteersRootElem.Add(new XElement("Volunteer", CreateVolunteerElement(item)));

        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }
}
