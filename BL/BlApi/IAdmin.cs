namespace BlApi
{
    public interface IAdmin
    {
        /// <summary>
        /// Gets the current system clock.
        /// </summary>
        /// <returns>The current date and time of the system clock.</returns>
        DateTime GetClock();

        /// <summary>
        /// Advances the system clock by a specified time unit.
        /// </summary>
        /// <param name="timeUnit">The time unit (e.g., seconds, minutes, hours, etc.) by which to advance the clock.</param>
        void PromoteClock(BO.Enums.TimeUnit timeUnit);

        /// <summary>
        /// Gets the current risk time range.
        /// </summary>
        /// <returns>The current risk time range as a TimeSpan.</returns>
        TimeSpan GetRiskTimeRange();

        /// <summary>
        /// Sets a new risk time range.
        /// </summary>
        /// <param name="riskTimeRange">The new risk time range to be set.</param>
        void SetRiskTimeRange(TimeSpan riskTimeRange);

        /// <summary>
        /// Resets the database to its initial state.
        /// </summary>
        void ResetDatabase();

        /// <summary>
        /// Initializes the database with default values.
        /// </summary>
        void InitializeDatabase();

        #region Stage 5
        void AddConfigObserver(Action configObserver);
        void RemoveConfigObserver(Action configObserver);
        void AddClockObserver(Action clockObserver);
        void RemoveClockObserver(Action clockObserver);
        #endregion Stage 5
    }
}

