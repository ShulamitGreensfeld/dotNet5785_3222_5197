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
        return AdminManager.Now;
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

        AdminManager.UpdateClock(newClock);
    }

    /// <summary>
    /// Retrieves the current risk time range from the database configuration.
    /// </summary>
    /// <returns>The current risk time range.</returns>
    public TimeSpan GetRiskTimeRange()
    {
        return AdminManager.RiskRange;
    }

    /// <summary>
    /// Sets a new risk time range in the database configuration.
    /// </summary>
    /// <param name="maxRange">The new risk time range to set.</param>
    public void SetRiskTimeRange(TimeSpan RiskRange)
    {
        AdminManager.RiskRange = RiskRange;
    }

    /// <summary>
    /// Resets the system's database and configuration to their initial state.
    /// </summary>
    public void ResetDatabase()
    {
        AdminManager.ResetDB();
        _dal.ResetDB();
        Console.WriteLine("Resetting configuration and clearing data...");
        Console.WriteLine("Resetting completed successfully!");
    }

    /// <summary>
    /// Initializes the system's database with test data and configurations.
    /// </summary>
    public void InitializeDatabase()
    {
        AdminManager.InitializeDB();
        _dal.ResetDB();
        DalTest.Initialization.Do();
    }

    #region Stage 5
    public void AddClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
   AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;
    #endregion Stage 5

}