namespace DalApi;
/// <summary>
/// Interface for system configuration, defining properties and methods
/// required for managing configuration settings.
/// </summary>
public interface IConfig
{  
        /// <summary>
        /// The system clock, used for tracking the simulation's current time.
        /// </summary>
        DateTime Clock { get; set; }

        /// <summary>
        /// The risk time range, indicating when a call is considered at risk.
        /// </summary>
        TimeSpan RiskRange { get; set; }

        /// <summary>
        /// Resets all configuration properties to their initial values.
        /// </summary>
        void Reset();
    }
}

}
