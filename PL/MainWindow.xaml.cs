using PL.Volunteer;
using System;
using System.Windows;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Main management window that displays the current time, allows advancing time units,
    /// updates risk range, and navigates to volunteer management screen.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Reference to the business logic layer (BL)
        /// </summary>
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        /// <summary>
        /// Dependency property for binding and displaying the current time in the UI
        /// </summary>
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        /// <summary>
        /// Current simulation time (used for display and advancing clock)
        /// </summary>
        public DateTime CurrentTime
        {
            get => (DateTime)GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        /// <summary>
        /// Dependency property for binding the risk time range (in years)
        /// </summary>
        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register("RiskRange", typeof(int), typeof(MainWindow));

        /// <summary>
        /// Number of years for the risk time range
        /// </summary>
        public int RiskRange
        {
            get => (int)GetValue(RiskRangeProperty);
            set => SetValue(RiskRangeProperty, value);
        }

        /// <summary>
        /// Constructor - initializes components, observers, and initial values
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Advances the clock by one minute
        /// </summary>
        private void btnAdvanceMinute_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Minute);

        /// <summary>
        /// Advances the clock by one hour
        /// </summary>
        private void btnAdvanceHour_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Hour);

        /// <summary>
        /// Advances the clock by one day
        /// </summary>
        private void btnAdvanceDay_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Day);

        /// <summary>
        /// Advances the clock by one month
        /// </summary>
        private void btnAdvanceMonth_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Month);

        /// <summary>
        /// Advances the clock by one year
        /// </summary>
        private void btnAdvanceYear_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Year);

        /// <summary>
        /// Updates the risk time range based on the current value of RiskRange
        /// </summary>
        private void btnUpdateRiskRange_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.SetRiskTimeRange(TimeSpan.FromDays(RiskRange * 365));
        }

        /// <summary>
        /// Observer method to update the UI with the current clock time
        /// </summary>
        private void clockObserver() =>
            Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());

        /// <summary>
        /// Observer method to update the risk range value from configuration
        /// </summary>
        private void configObserver() =>
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;

        /// <summary>
        /// Cleanup when window is closed: remove clock and config observers
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        /// <summary>
        /// Loads initial values when window is loaded: time, risk range, observers
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTime = s_bl.Admin.GetClock();
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        /// <summary>
        /// Handles window Closed event (alternative to OnClosed)
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        /// <summary>
        /// Opens the volunteer management window
        /// </summary>
        private void btnVolunteers_Click(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show();
        }
    }
}