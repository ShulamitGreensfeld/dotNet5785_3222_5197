using System.Xml.Linq;
using System.Runtime.CompilerServices; // נדרש עבור MethodImpl

namespace Dal;

/// <summary>
/// This class manages system-wide configuration data, including file paths,
/// unique ID generators, system clock, and risk range settings.
/// </summary>
internal static class Config
{
    /// <summary>
    /// Path to the XML configuration file that stores system data settings.
    /// </summary>
    internal const string s_data_config_xml = "data-config.xml";

    /// <summary>
    /// Path to the XML file that stores assignments data.
    /// </summary>
    internal const string s_assignments_xml = "assignments.xml";

    /// <summary>
    /// Path to the XML file that stores calls data.
    /// </summary>
    internal const string s_calls_xml = "calls.xml";

    /// <summary>
    /// Path to the XML file that stores volunteers data.
    /// </summary>
    internal const string s_volunteers_xml = "volunteers.xml";

    /// <summary>
    /// Retrieves and increments the next available assignment ID from the XML file.
    /// Used to generate unique IDs for new assignments.
    /// </summary>
    internal static int NextAssignmentId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "nextAssignmentId");

        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "nextAssignmentId", value);
    }

    /// <summary>
    /// Retrieves and increments the next available call ID from the XML file.
    /// Used to generate unique IDs for new calls.
    /// </summary>
    internal static int NextCallId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "nextCallId");

        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "nextCallId", value);
    }

    /// <summary>
    /// Gets or sets the risk range time span from the XML file.
    /// This represents a system-defined period of risk.
    /// </summary>
    internal static TimeSpan RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }

    /// <summary>
    /// Gets or sets the system clock value from the XML file.
    /// This represents the current operational time for the system.
    /// </summary>
    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    /// <summary>
    /// Resets the configuration settings to their default values by loading them
    /// from the "default-config.xml" file.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        XElement defaultConfig = XMLTools.LoadListFromXMLElement("default-config.xml");
        NextCallId = defaultConfig.ToIntNullable("nextCallId") ?? 1000;
        NextAssignmentId = defaultConfig.ToIntNullable("nextAssignmentId") ?? 2000;
        Clock = defaultConfig.ToDateTimeNullable("Clock") ?? DateTime.Now;
        RiskRange = defaultConfig.ToTimeSpanNullable("RiskRange") ?? TimeSpan.FromHours(2);
    }
}
