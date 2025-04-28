namespace Dal;
using DalApi;
using System.Reflection.Metadata.Ecma335;

/// <summary>
/// Implementation of the IConfig interface, providing access to system-wide 
/// configuration settings such as the system clock and risk range.
/// </summary>
internal class ConfigImplementation : IConfig
{
    /// <summary>
    /// Gets or sets the system clock, which represents the current operational time.
    /// </summary>
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    /// <summary>
    /// Resets the configuration settings to their default values.
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }

    /// <summary>
    /// Gets or sets the risk range time span, representing a system-defined period of risk.
    /// </summary>
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }
  
}
