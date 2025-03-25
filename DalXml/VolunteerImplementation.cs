namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// Implementation of the IVolunteer interface, managing volunteer records stored in an XML file.
/// </summary>
internal class VolunteerImplementation : IVolunteer
{
    /// <summary>
    /// Converts an XElement representing a volunteer to a Volunteer object.
    /// </summary>
    static Volunteer getVolunteer(XElement v)
    {
        return new DO.Volunteer()
        {
            ID = v.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            Name = (string?)v.Element("Name") ?? "",
            Phone = (string?)v.Element("Phone") ?? "",
            Email = (string?)v.Element("Email") ?? "",
            IsActive = (bool?)v.Element("IsActive") ?? false,
            Role = Enum.TryParse<Role>((string)v.Element("Role")! ?? "", out var role) ? role : Role.Volunteer,
            Password = (string?)v.Element("Password") ?? null,
            Address = (string?)v.Element("Address") ?? "",
            Latitude = double.TryParse((string?)v.Element("Latitude"), out double lat) ? lat : 0.0,
            Longitude = double.TryParse((string?)v.Element("Longitude"), out double lon) ? lon : 0.0,
            MaxDistanceForCall = double.TryParse((string?)v.Element("LargestDistance"), out double maxDist) ? maxDist : 0.0,
            DistanceType = Enum.TryParse<DistanceType>((string)v.Element("DistanceType")! ?? "", out var distance) ? distance : DistanceType.WalkingDistance,
        };
    }

    /// <summary>
    /// Creates a new volunteer record in the XML file.
    /// </summary>
    public void Create(Volunteer item)
    {
        XElement volunteerElement = CreateVolunteerElement(item);
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        volunteersRootElem.Add(volunteerElement);
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Converts a Volunteer object into an XElement for storage in the XML file.
    /// </summary>
    private XElement CreateVolunteerElement(Volunteer item)
    {
        return new XElement("Volunteer",
            new XElement("Id", item.ID),
            new XElement("Name", item.Name),
            new XElement("Phone", item.Phone),
            new XElement("Email", item.Email),
            new XElement("IsActive", item.IsActive),
            new XElement("Role", item.Role.ToString()),
            new XElement("Address", string.IsNullOrEmpty(item.Address) ? "" : item.Address),
            new XElement("Latitude", item.Latitude?.ToString() ?? string.Empty),
            new XElement("Longitude", item.Longitude?.ToString() ?? string.Empty),
            new XElement("LargestDistance", item.MaxDistanceForCall),
            new XElement("DistanceType", item.DistanceType.ToString()),
            string.IsNullOrEmpty(item.Password) ? null : new XElement("Password", item.Password)
        );
    }

    /// <summary>
    /// Deletes a volunteer by their ID from the XML file.
    /// </summary>
    public void Delete(int id)
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        XElement volunteerElem = volunteersRootElem.Elements("Volunteer")
            .FirstOrDefault(v => (int)v.Element("Id")! == id)!;
        if (volunteerElem == null)
        {
            throw new DalDoesNotExistException($"Volunteer with ID={id} not found.");
        }
        volunteerElem.Remove();
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Deletes all volunteers from the XML file.
    /// </summary>
    public void DeleteAll()
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        volunteersRootElem.RemoveAll();
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Reads a volunteer by their ID from the XML file.
    /// </summary>
    public Volunteer? Read(int id)
    {
        XElement? volunteerElem =
        XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return volunteerElem is null ? null : getVolunteer(volunteerElem);
    }

    /// <summary>
    /// Reads a single volunteer that matches the given filter.
    /// </summary>
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().Select(v => getVolunteer(v)).FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all volunteers, optionally filtering the results based on a given condition.
    /// </summary>
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
                Latitude = double.TryParse((string?)v.Element("Latitude"), out double lat) ? lat : 0.0,
                Longitude = double.TryParse((string?)v.Element("Longitude"), out double lon) ? lon : 0.0,
                MaxDistanceForCall = (double)v.Element("LargestDistance")!,
                DistanceType = (DistanceType)Enum.Parse(typeof(DistanceType), (string)v.Element("DistanceType")!)
            })
            .ToList();

        return filter == null ? volunteers : volunteers.Where(filter);
    }

    /// <summary>
    /// Updates an existing volunteer in the XML file with new information.
    /// </summary>
    public void Update(Volunteer item)
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        (volunteersRootElem.Elements().FirstOrDefault(v => (int?)v.Element("Id") == item.ID)
        ?? throw new DO.DalDoesNotExistException($"Volunteer with ID={item.ID} does Not exist")).Remove();
        volunteersRootElem.Add(CreateVolunteerElement(item));
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }
}
