using System.Xml.Linq;

namespace Dal;
internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";//stage 3
    internal const string s_assignments_xml = "assignments.xml";//stage 3
    internal const string s_calls_xml = "calls.xml";//stage 3
    internal const string s_volunteers_xml = "volunteers.xml";//stage 3


    internal static int NextAssignmentId//stage 3
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "nextAssignmentId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "nextAssignmentId", value);
    }

    internal static int NextCallId//stage 3
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "nextCallId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "nextCallId", value);
    }

    internal static TimeSpan RiskRange//stage 3
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }

    internal static DateTime Clock//stage 3
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    internal static void Reset()//stage 3
    {
        XElement defaultConfig = XMLTools.LoadListFromXMLElement("default-config.xml");
        NextCallId = defaultConfig.ToIntNullable("nextCallId") ?? 1000;
        NextAssignmentId = defaultConfig.ToIntNullable("nextAssignmentId") ?? 2000;
        Clock = defaultConfig.ToDateTimeNullable("Clock") ?? DateTime.Now;
        RiskRange = defaultConfig.ToTimeSpanNullable("RiskRange") ?? TimeSpan.FromHours(2);
    }
}