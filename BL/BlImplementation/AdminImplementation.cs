using BlApi;
using Helpers;
using static BO.Enums;

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
    public void PromoteClock(TimeUnit timeUnit)
    {
        DateTime oldClock = GetClock();
        DateTime newClock = oldClock;

        switch (timeUnit)
        {
            case TimeUnit.Minute:
                newClock = oldClock.AddMinutes(1);
                break;
            case TimeUnit.Hour:
                newClock = oldClock.AddHours(1);
                break;
            case TimeUnit.Day:
                newClock = oldClock.AddDays(1);
                break;
            case TimeUnit.Month:
                newClock = oldClock.AddMonths(1);
                break;
            case TimeUnit.Year:
                newClock = oldClock.AddYears(1);
                break;
        }

        //ClockManager.UpdateClock(newClock); 
        CallManager.PeriodicVolunteersUpdates(oldClock, newClock);
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