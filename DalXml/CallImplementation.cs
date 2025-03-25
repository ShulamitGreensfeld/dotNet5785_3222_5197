namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

internal class CallImplementation : ICall
{
    /// <summary>
    /// Creates a new call and assigns it a unique ID.
    /// The new call is then added to the XML file.
    /// </summary>
    public void Create(Call item)
    {
        int newId = XMLTools.GetAndIncreaseConfigIntVal("data-config.xml", "nextCallId");
        Call callCopy = item with { ID = newId };
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Calls.Add(callCopy);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }

    /// <summary>
    /// Reads all calls from the XML file.
    /// If a filter function is provided, only matching calls are returned.
    /// </summary>
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return filter != null ? Calls.Where(filter) : Calls;
    }

    /// <summary>
    /// Deletes a call by its ID.
    /// Throws an exception if the call does not exist.
    /// </summary>
    public void Delete(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Call? existingCall = Read(id);
        if (existingCall == null)
            throw new DalDeletionImpossibleException($"Call with ID={id} does not exist");

        Calls.Remove(existingCall);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }

    /// <summary>
    /// Deletes all calls from the XML file.
    /// </summary>
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Call>(), Config.s_calls_xml);
    }

    /// <summary>
    /// Reads a single call by its ID.
    /// Returns null if the call does not exist.
    /// </summary>
    public Call? Read(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return Calls.FirstOrDefault(call => call.ID == id);
    }

    /// <summary>
    /// Updates an existing call.
    /// Throws an exception if the call does not exist.
    /// </summary>
    public void Update(Call item)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Call? existingCall = Read(item.ID);
        if (existingCall == null)
            throw new DalDoesNotExistException($"Call with ID={item.ID} does not exist");

        Calls.Remove(existingCall);
        Calls.Add(item);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }

    /// <summary>
    /// Reads a call based on a given filter function.
    /// Returns null if no matching call is found.
    /// Throws an exception if the filter function is null.
    /// </summary>
    public Call? Read(Func<Call, bool> filter)
    {
        if (filter == null)
            throw new NullException($"{nameof(filter)} function cannot be null");

        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return Calls.FirstOrDefault(filter);
    }
}
