using PL.Volunteer;
using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static BO.Enums;
using BlApi;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Main management window that displays the current time, allows advancing time units,
    /// updates risk range, and navigates to volunteer/call management screens.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Reference to the business logic layer (BL)
        /// </summary>
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


        private bool _isSimulatorRunning = false;
        private int _simulatorSpeed = 1;
        private CancellationTokenSource? _cancellationTokenSource;
        public int SimulatorSpeed
        {
            get => _simulatorSpeed;
            set
            {
                if (value > 0)
                    _simulatorSpeed = value;
                else
                    MessageBox.Show("Speed must be greater than 0.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

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
        /// Collection for displaying number of calls per status
        /// </summary>
        public ObservableCollection<CallQuantity> CallQuantities { get; set; }

        /// <summary>
        /// Command to view calls by selected status
        /// </summary>
        public ICommand ViewCallsCommand { get; }

        /// <summary>
        /// Constructor - initializes components, observers, and initial values
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize call status view
            CallQuantities = new ObservableCollection<CallQuantity>();
            LoadCallQuantities();
            ViewCallsCommand = new RelayCommand(ViewCallsByStatus);

            // Initialize simulation controls
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

        private void btnAdvanceHour_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Hour);

        private void btnAdvanceDay_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Day);

        private void btnAdvanceMonth_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Month);

        private void btnAdvanceYear_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Year);

        private void btnUpdateRiskRange_Click(object sender, RoutedEventArgs e) =>
            s_bl.Admin.SetRiskTimeRange(TimeSpan.FromDays(RiskRange * 365));

        private void clockObserver() =>
            Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());

        private void configObserver() =>
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTime = s_bl.Admin.GetClock();
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

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
            if (Application.Current.Windows.OfType<VolunteerListWindow>().Any())
            {
                MessageBox.Show("Volunteer management window is already open.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // פתיחת חלון ניהול מתנדבים
            var volunteerListWindow = new VolunteerListWindow();
            volunteerListWindow.Show();
        }

        /// <summary>
        /// Opens the call management window (only one at a time)
        /// </summary>
        private void btnCalls_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<CallManagementWindow>().Any())
            {
                MessageBox.Show("Call management window is already open.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (Application.Current.Windows.OfType<CallManagementWindow>().Any())
            {
                MessageBox.Show("Call management window is already open.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var callManagementWindow = new CallManagementWindow();
            callManagementWindow.Show();
        }

        /// <summary>
        /// Load call quantities from BL and update observable collection
        /// </summary>
        private void LoadCallQuantities()
        {
            try
            {
                var quantities = s_bl.Call.GetCallQuantitiesByStatus();
                CallQuantities.Clear();
                foreach (CallStatus status in Enum.GetValues(typeof(CallStatus)))
                {
                    CallQuantities.Add(new CallQuantity
                    {
                        Status = status,
                        Quantity = quantities[(int)status]
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading call quantities: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// View list of calls filtered by selected status
        /// </summary>
        private void ViewCallsByStatus(object parameter)
        {
            if (parameter is CallStatus status)
            {
                // פתיחת חלון ניהול קריאות עם רשימה מסוננת לפי הסטטוס שנבחר
                var filteredCallManagementWindow = new FilteredCallManagementWindow(status);
                filteredCallManagementWindow.Show();
            }
            else
            {
                MessageBox.Show("Invalid status selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Helper class to hold call quantity per status
        /// </summary>
        public class CallQuantity
        {
            public CallStatus Status { get; set; }
            public int Quantity { get; set; }
        }

        private void btnStartSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (_isSimulatorRunning)
            {
                MessageBox.Show("Simulator is already running.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isSimulatorRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => RunSimulator(_cancellationTokenSource.Token));
            MessageBox.Show("Simulator started successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void btnStopSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSimulatorRunning)
            {
                MessageBox.Show("Simulator is not running.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _cancellationTokenSource?.Cancel();
            _isSimulatorRunning = false;
            MessageBox.Show("Simulator stopped successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void btnSetSpeed_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSimulatorRunning)
            {
                MessageBox.Show("Simulator is not running. Start the simulator first.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show($"Simulator speed set to {SimulatorSpeed}x.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task RunSimulator(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(1000 / SimulatorSpeed, token); // Adjust delay based on speed
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Minute);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error promoting clock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void btnResetDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to reset the database? This action cannot be undone.",
                                             "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    s_bl.Admin.ResetDatabase();
                    MessageBox.Show("Database has been reset successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnInitializeDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to initialize the database? This will overwrite existing data.",
                                             "Confirm Initialization", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    s_bl.Admin.InitializeDatabase();
                    MessageBox.Show("Database has been initialized successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}