//using PL.Volunteer;
//using System;
//using System.Linq;
//using System.Windows;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Collections.ObjectModel;
//using System.Windows.Input;
//using static BO.Enums;
//using BlApi;
//using System.ComponentModel;
//using PL.Commands;

//namespace PL
//{
//    /// <summary>
//    /// Main management window of the application.
//    /// Handles simulation, time control, risk range, database operations and volunteer/call management.
//    /// </summary>
//    public partial class MainWindow : Window, INotifyPropertyChanged
//    {
//        private static readonly IBl s_bl = Factory.Get();

//        private bool _isSimulatorRunning = false;
//        private int _simulatorSpeed = 1;
//        private CancellationTokenSource? _cancellationTokenSource;
//        private readonly Action _refreshCallQuantities;

//        public int SimulatorSpeed
//        {
//            get => _simulatorSpeed;
//            set
//            {
//                if (value > 0)
//                {
//                    _simulatorSpeed = value;
//                    OnPropertyChanged(nameof(SimulatorSpeed));
//                }
//                else
//                {
//                    ShowError("Speed must be greater than 0.");
//                }
//            }
//        }

//        // DependencyProperty for current time
//        public static readonly DependencyProperty CurrentTimeProperty =
//            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

//        public DateTime CurrentTime
//        {
//            get => (DateTime)GetValue(CurrentTimeProperty);
//            set => SetValue(CurrentTimeProperty, value);
//        }

//        // DependencyProperty for risk range
//        public static readonly DependencyProperty RiskRangeProperty =
//            DependencyProperty.Register("RiskRange", typeof(int), typeof(MainWindow));

//        public int RiskRange
//        {
//            get => (int)GetValue(RiskRangeProperty);
//            set => SetValue(RiskRangeProperty, value);
//        }

//        public ObservableCollection<CallQuantity> CallQuantities { get; set; } = new();

//        // Command bindings
//        public ICommand ViewCallsCommand { get; }
//        public ICommand PromoteMinuteCommand { get; }
//        public ICommand PromoteHourCommand { get; }
//        public ICommand PromoteDayCommand { get; }
//        public ICommand PromoteMonthCommand { get; }
//        public ICommand PromoteYearCommand { get; }
//        public ICommand UpdateRiskRangeCommand { get; }
//        public ICommand InitializeDatabaseCommand { get; }
//        public ICommand ResetDatabaseCommand { get; }
//        public ICommand OpenVolunteersCommand { get; }
//        public ICommand OpenCallsCommand { get; }
//        public ICommand StartSimulatorCommand { get; }
//        public ICommand StopSimulatorCommand { get; }
//        public ICommand SetSpeedCommand { get; }

//        public event PropertyChangedEventHandler? PropertyChanged;
//        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

//        // Constructor
//        public MainWindow()
//        {
//            InitializeComponent();

//            // Initialize commands
//            ViewCallsCommand = new RelayCommand(ViewCallsByStatus);
//            PromoteMinuteCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Minute));
//            PromoteHourCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Hour));
//            PromoteDayCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Day));
//            PromoteMonthCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Month));
//            PromoteYearCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Year));
//            UpdateRiskRangeCommand = new RelayCommand(_ => s_bl.Admin.SetRiskTimeRange(TimeSpan.FromDays(RiskRange * 365)));

//            InitializeDatabaseCommand = new RelayCommand(_ => ConfirmAndRun("Initialize", s_bl.Admin.InitializeDatabase));
//            ResetDatabaseCommand = new RelayCommand(_ => ConfirmAndRun("Reset", s_bl.Admin.ResetDatabase));

//            OpenVolunteersCommand = new RelayCommand(_ => OpenWindow<VolunteerListWindow>());
//            OpenCallsCommand = new RelayCommand(_ => OpenWindow<CallManagementWindow>());

//            StartSimulatorCommand = new RelayCommand(_ => StartSimulator());
//            StopSimulatorCommand = new RelayCommand(_ => StopSimulator());
//            SetSpeedCommand = new RelayCommand(_ => SetSimulatorSpeed());

//            // Set initial properties
//            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
//            CurrentTime = s_bl.Admin.GetClock();

//            // Register observers
//            s_bl.Admin.AddClockObserver(clockObserver);
//            s_bl.Admin.AddConfigObserver(configObserver);

//            // Setup call quantity observer
//            _refreshCallQuantities = LoadCallQuantities;
//            s_bl.Call.AddObserver(_refreshCallQuantities);
//            LoadCallQuantities();

//            DataContext = this; // Should ideally be moved to XAML
//        }

//        // Called whenever clock is updated
//        private void clockObserver() =>
//            Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());

//        // Called whenever config (risk range) is updated
//        private void configObserver() =>
//            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;

//        // Loads and refreshes call quantity data
//        private void LoadCallQuantities()
//        {
//            try
//            {
//                var quantities = s_bl.Call.GetCallQuantitiesByStatus();
//                CallQuantities.Clear();
//                foreach (CallStatus status in Enum.GetValues(typeof(CallStatus)))
//                {
//                    CallQuantities.Add(new CallQuantity
//                    {
//                        Status = status,
//                        Quantity = quantities[(int)status]
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                ShowError($"Error loading call quantities: {ex.Message}");
//            }
//        }

//        // Opens call management filtered by status
//        private void ViewCallsByStatus(object parameter)
//        {
//            if (parameter is BO.Enums.CallStatus status)
//                new CallManagementWindow((int)status).Show();
//            else
//                MessageBox.Show("Invalid status selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//        }

//        // Starts the call simulator
//        private void StartSimulator()
//        {
//            if (_isSimulatorRunning)
//            {
//                MessageBox.Show("Simulator is already running.");
//                return;
//            }

//            _isSimulatorRunning = true;
//            _cancellationTokenSource = new CancellationTokenSource();
//            Task.Run(() => RunSimulator(_cancellationTokenSource.Token));
//            MessageBox.Show("Simulator started.");
//        }

//        // Stops the call simulator
//        private void StopSimulator()
//        {
//            if (!_isSimulatorRunning)
//            {
//                MessageBox.Show("Simulator is not running.");
//                return;
//            }

//            _cancellationTokenSource?.Cancel();
//            _isSimulatorRunning = false;
//            MessageBox.Show("Simulator stopped.");
//        }

//        // Sets the simulation speed (visual only)
//        private void SetSimulatorSpeed()
//        {
//            if (!_isSimulatorRunning)
//            {
//                MessageBox.Show("Simulator not running.");
//                return;
//            }

//            MessageBox.Show($"Simulator speed set to {SimulatorSpeed}x.");
//        }

//        // Background task for simulating call progression
//        private async Task RunSimulator(CancellationToken token)
//        {
//            try
//            {
//                while (!token.IsCancellationRequested)
//                {
//                    await Task.Delay(1000 / SimulatorSpeed, token);
//                    Dispatcher.Invoke(() => s_bl.Admin.PromoteClock(TimeUnit.Minute));
//                }
//            }
//            catch (TaskCanceledException) { }
//            catch (Exception ex)
//            {
//                ShowError($"Error in simulator: {ex.Message}");
//            }
//        }

//        // Displays confirmation before executing a database operation
//        private void ConfirmAndRun(string actionName, Action dbAction)
//        {
//            var result = MessageBox.Show(
//                $"Are you sure you want to {actionName.ToLower()} the database?",
//                $"Confirm {actionName}",
//                MessageBoxButton.YesNo,
//                MessageBoxImage.Warning);

//            if (result == MessageBoxResult.Yes)
//            {
//                try
//                {
//                    Mouse.OverrideCursor = Cursors.Wait;
//                    dbAction();
//                    MessageBox.Show($"Database {actionName.ToLower()}ed successfully.", "Success");
//                }
//                catch (Exception ex)
//                {
//                    ShowError(ex.Message);
//                }
//                finally
//                {
//                    Mouse.OverrideCursor = null;
//                }
//            }
//        }

//        // Opens a window only if it's not already open
//        private void OpenWindow<T>() where T : Window, new()
//        {
//            if (Application.Current.Windows.OfType<T>().Any())
//            {
//                MessageBox.Show($"{typeof(T).Name} is already open.");
//                return;
//            }
//            new T().Show();
//        }

//        // Shows error messages in a message box
//        private void ShowError(string msg) =>
//            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

//        // Internal class for call quantity display
//        public class CallQuantity
//        {
//            public CallStatus Status { get; set; }
//            public int Quantity { get; set; }
//        }

//        // Unsubscribes observers on window close
//        protected override void OnClosed(EventArgs e)
//        {
//            base.OnClosed(e);
//            s_bl.Admin.RemoveClockObserver(clockObserver);
//            s_bl.Admin.RemoveConfigObserver(configObserver);
//            s_bl.Call.RemoveObserver(_refreshCallQuantities);
//            App.IsAdminLoggedIn = false;
//            s_bl.Volunteer.LogoutVolunteer(0);
//        }
//    }
//}

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
using System.ComponentModel;
using PL.Commands;

namespace PL
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        private bool _isSimulatorRunning = false;
        private int _simulatorSpeed = 1;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly Action _refreshCallQuantities;

        public int SimulatorSpeed
        {
            get => _simulatorSpeed;
            set
            {
                if (value > 0)
                {
                    _simulatorSpeed = value;
                    OnPropertyChanged(nameof(SimulatorSpeed));
                }
                else
                {
                    ShowError("Speed must be greater than 0.");
                }
            }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        public DateTime CurrentTime
        {
            get => (DateTime)GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register("RiskRange", typeof(int), typeof(MainWindow));

        public int RiskRange
        {
            get => (int)GetValue(RiskRangeProperty);
            set => SetValue(RiskRangeProperty, value);
        }

        public ObservableCollection<CallQuantity> CallQuantities { get; set; } = new();

        public ICommand ViewCallsCommand { get; }
        public ICommand PromoteMinuteCommand { get; }
        public ICommand PromoteHourCommand { get; }
        public ICommand PromoteDayCommand { get; }
        public ICommand PromoteMonthCommand { get; }
        public ICommand PromoteYearCommand { get; }
        public ICommand UpdateRiskRangeCommand { get; }
        public ICommand InitializeDatabaseCommand { get; }
        public ICommand ResetDatabaseCommand { get; }
        public ICommand OpenVolunteersCommand { get; }
        public ICommand OpenCallsCommand { get; }
        public ICommand StartSimulatorCommand { get; }
        public ICommand StopSimulatorCommand { get; }
        public ICommand SetSpeedCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainWindow()
        {
            InitializeComponent();

            ViewCallsCommand = new RelayCommand(ViewCallsByStatus);
            PromoteMinuteCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Minute));
            PromoteHourCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Hour));
            PromoteDayCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Day));
            PromoteMonthCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Month));
            PromoteYearCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Year));
            UpdateRiskRangeCommand = new RelayCommand(_ => s_bl.Admin.SetRiskTimeRange(TimeSpan.FromDays(RiskRange * 365)));

            InitializeDatabaseCommand = new RelayCommand(_ => ConfirmAndRun("Initialize", s_bl.Admin.InitializeDatabase));
            ResetDatabaseCommand = new RelayCommand(_ => ConfirmAndRun("Reset", s_bl.Admin.ResetDatabase));

            OpenVolunteersCommand = new RelayCommand(_ => OpenWindow<VolunteerListWindow>());
            OpenCallsCommand = new RelayCommand(_ => new CallManagementWindow(App.LoggedAdminId).Show());

            StartSimulatorCommand = new RelayCommand(_ => StartSimulator());
            StopSimulatorCommand = new RelayCommand(_ => StopSimulator());
            SetSpeedCommand = new RelayCommand(_ => SetSimulatorSpeed());

            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
            CurrentTime = s_bl.Admin.GetClock();

            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);

            _refreshCallQuantities = LoadCallQuantities;
            s_bl.Call.AddObserver(_refreshCallQuantities);
            LoadCallQuantities();

            DataContext = this;
        }

        private void clockObserver() =>
            Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());

        private void configObserver() =>
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;

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
                ShowError($"Error loading call quantities: {ex.Message}");
            }
        }

        //private void ViewCallsByStatus(object parameter)
        //{
        //    if (parameter is CallStatus status)
        //        new CallManagementWindow(-1,status).Show();
        //    else
        //        MessageBox.Show("Invalid status selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
        private void ViewCallsByStatus(object parameter)
        {
            if (parameter is CallStatus status)
            {
                if (App.LoggedAdminId > 0)
                    new CallManagementWindow(App.LoggedAdminId, status).Show();
                else
                    MessageBox.Show("No admin is logged in. Please log in first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Invalid status selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartSimulator()
        {
            if (_isSimulatorRunning)
            {
                MessageBox.Show("Simulator is already running.");
                return;
            }

            _isSimulatorRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => RunSimulator(_cancellationTokenSource.Token));
            MessageBox.Show("Simulator started.");
        }

        private void StopSimulator()
        {
            if (!_isSimulatorRunning)
            {
                MessageBox.Show("Simulator is not running.");
                return;
            }

            _cancellationTokenSource?.Cancel();
            _isSimulatorRunning = false;
            MessageBox.Show("Simulator stopped.");
        }

        private void SetSimulatorSpeed()
        {
            if (!_isSimulatorRunning)
            {
                MessageBox.Show("Simulator not running.");
                return;
            }

            MessageBox.Show($"Simulator speed set to {SimulatorSpeed}x.");
        }

        private async Task RunSimulator(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(1000 / SimulatorSpeed, token);
                    Dispatcher.Invoke(() => s_bl.Admin.PromoteClock(TimeUnit.Minute));
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                ShowError($"Error in simulator: {ex.Message}");
            }
        }

        private void ConfirmAndRun(string actionName, Action dbAction)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to {actionName.ToLower()} the database?",
                $"Confirm {actionName}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    dbAction();
                    MessageBox.Show($"Database {actionName.ToLower()}ed successfully.", "Success");
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void OpenWindow<T>() where T : Window, new()
        {
            if (Application.Current.Windows.OfType<T>().Any())
            {
                MessageBox.Show($"{typeof(T).Name} is already open.");
                return;
            }
            new T().Show();
        }

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        public class CallQuantity
        {
            public CallStatus Status { get; set; }
            public int Quantity { get; set; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
            s_bl.Call.RemoveObserver(_refreshCallQuantities);

            App.IsAdminLoggedIn = false;

            // ✅ log out the manager from BL if ID was stored
            if (App.LoggedAdminId > 0)
                s_bl.Volunteer.LogoutVolunteer(App.LoggedAdminId);
        }
    }
}