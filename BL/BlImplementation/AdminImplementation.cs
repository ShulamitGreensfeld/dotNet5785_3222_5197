using BlApi;
using Helpers;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    /// <summary>
    /// Retrieves the current system clock time.
    /// </summary>
    /// <returns>The current system time.</returns>
    public DateTime GetClock()
    {
        return ClockManager.Now;
    }

    /// <summary>
    /// Advances the system clock by a specified time unit (minute, hour, day, month, or year).
    /// </summary>
    /// <param name="timeUnit">The unit of time to advance (minute, hour, day, month, or year).</param>
    public void PromoteClock(BO.Enums.TimeUnit timeUnit)
    {
        DateTime newClock = timeUnit switch
        {
            BO.Enums.TimeUnit.Minute => ClockManager.Now.AddMinutes(1),
            BO.Enums.TimeUnit.Hour => ClockManager.Now.AddHours(1),
            BO.Enums.TimeUnit.Day => ClockManager.Now.AddDays(1),
            BO.Enums.TimeUnit.Month => ClockManager.Now.AddMonths(1),
            BO.Enums.TimeUnit.Year => ClockManager.Now.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), "Invalid time unit")
        };

        ClockManager.UpdateClock(newClock);
    }

    /// <summary>
    /// Retrieves the current risk time range from the database configuration.
    /// </summary>
    /// <returns>The current risk time range.</returns>
    public TimeSpan GetRiskTimeRange()
    {
        return _dal.Config.RiskRange;
    }

    /// <summary>
    /// Sets a new risk time range in the database configuration.
    /// </summary>
    /// <param name="riskTimeRange">The new risk time range to set.</param>
    public void SetRiskTimeRange(TimeSpan riskTimeRange)
    {
        _dal.Config.RiskRange = riskTimeRange;
    }

    /// <summary>
    /// Resets the system's database and configuration to their initial state.
    /// </summary>
    public void ResetDatabase()
    {
        _dal.Config.Reset();
        _dal.ResetDB();
        Console.WriteLine("Resetting configuration and clearing data...");
        Console.WriteLine("Resetting completed successfully!");
    }

    /// <summary>
    /// Initializes the system's database with test data and configurations.
    /// </summary>
    public void InitializeDatabase()
    {
        _dal.ResetDB();
        DalTest.Initialization.Do();
    }
}
