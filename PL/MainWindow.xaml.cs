using PL.Volunteer;
using System;
using System.Windows;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Static readonly field for accessing the BL layer
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // Define the Dependency Property
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        // CLR Wrapper for the Dependency Property
        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        private void btnAdvanceMinute_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Minute);
        }

        private void btnAdvanceHour_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Hour);
        }

        private void btnAdvanceDay_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Day);
        }

        private void btnAdvanceMonth_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Month);
        }

        private void btnAdvanceYear_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Year);
        }

        // Define the Dependency Property
        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register("RiskRange", typeof(int), typeof(MainWindow));

        // CLR Wrapper for the Dependency Property
        public int RiskRange
        {
            get { return (int)GetValue(RiskRangeProperty); }
            set { SetValue(RiskRangeProperty, value); }
        }

        // Button Click Event Handler
        private void btnUpdateRiskRange_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.SetRiskTimeRange(TimeSpan.FromDays(RiskRange * 365)); // Example logic
        }

        private void clockObserver()
        {    // Update the CurrentTime property with the new clock value
            Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());
        }
        private void configObserver()
        {
            // Update the RiskRange property with the new risk time range in years
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
        }
        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            // Initialize the RiskRange property
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365; // Example logic
                                                                            // Register observers
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
            Loaded += MainWindow_Loaded; // Register the Loaded event
        }
        // Destructor or OnClosed method
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Unregister observers to prevent memory leaks
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }
        // Define the Loaded event handler
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the current system clock value to the CurrentTime dependency property
            CurrentTime = s_bl.Admin.GetClock();

            // Set the RiskRange dependency property based on the current risk time range
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;

            // Add observers for clock and configuration updates
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        // Define the Closed event handler
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Remove the clock observer
            s_bl.Admin.RemoveClockObserver(clockObserver);

            // Remove the configuration observer
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }
        private void btnVolunteers_Click(object sender, RoutedEventArgs e)
        {
            // Open the Volunteer List Window and allow access to the main window
            new VolunteerListWindow().Show();
        }


    }


}
