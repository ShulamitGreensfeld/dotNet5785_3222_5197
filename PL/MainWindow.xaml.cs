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
using System.Windows.Threading;

namespace PL
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public static readonly DependencyProperty IsSimulatorRunningProperty =
            DependencyProperty.Register(nameof(IsSimulatorRunning), typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
        public bool IsSimulatorRunning
        {
            get => (bool)GetValue(IsSimulatorRunningProperty);
            set
            {
                SetValue(IsSimulatorRunningProperty, value);
                OnPropertyChanged(nameof(IsSimulatorRunning));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register(nameof(Interval), typeof(int), typeof(MainWindow), new PropertyMetadata(1));
        public int Interval
        {
            get => (int)GetValue(IntervalProperty);
            set
            {
                if (value > 0)
                {
                    SetValue(IntervalProperty, value);
                    OnPropertyChanged(nameof(Interval));
                }
                else
                    ShowError("Interval must be greater than 0.");
            }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now));
        public DateTime CurrentTime
        {
            get => (DateTime)GetValue(CurrentTimeProperty);
            set { SetValue(CurrentTimeProperty, value); OnPropertyChanged(nameof(CurrentTime)); }
        }

        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register(nameof(RiskRange), typeof(int), typeof(MainWindow), new PropertyMetadata(1));
        public int RiskRange
        {
            get => (int)GetValue(RiskRangeProperty);
            set { SetValue(RiskRangeProperty, value); OnPropertyChanged(nameof(RiskRange)); }
        }

        public ObservableCollection<CallQuantity> CallQuantities { get; set; } = new();

        public ICommand StartStopSimulatorCommand { get; }
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

        private CancellationTokenSource? _cancellationTokenSource;
        private volatile DispatcherOperation? _clockObserverOperation = null;
        private volatile DispatcherOperation? _refreshCallQuantitiesOperation = null;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainWindow()
        {
            InitializeComponent();

            StartStopSimulatorCommand = new RelayCommand(_ => ToggleSimulator());
            ViewCallsCommand = new RelayCommand(ViewCallsByStatus);
            PromoteMinuteCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Minute), _ => !IsSimulatorRunning);
            PromoteHourCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Hour), _ => !IsSimulatorRunning);
            PromoteDayCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Day), _ => !IsSimulatorRunning);
            PromoteMonthCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Month), _ => !IsSimulatorRunning);
            PromoteYearCommand = new RelayCommand(_ => s_bl.Admin.PromoteClock(TimeUnit.Year), _ => !IsSimulatorRunning);
            UpdateRiskRangeCommand = new RelayCommand(_ => s_bl.Admin.SetRiskTimeRange(TimeSpan.FromMinutes(RiskRange)), _ => !IsSimulatorRunning);
            InitializeDatabaseCommand = new RelayCommand(_ => ConfirmAndRun("Initialize", s_bl.Admin.InitializeDatabase), _ => !IsSimulatorRunning);
            ResetDatabaseCommand = new RelayCommand(_ => ConfirmAndRun("Reset", s_bl.Admin.ResetDatabase), _ => !IsSimulatorRunning);
            //OpenVolunteersCommand = new RelayCommand(_ => OpenWindow<VolunteerListWindow>());
            //OpenCallsCommand = new RelayCommand(_ => new CallManagementWindow(App.LoggedAdminId).Show());
            OpenCallsCommand = new RelayCommand(_ =>
            {
                if (IsSimulatorRunning)
                    new CallManagementWindow(App.LoggedAdminId).Show();
                else
                    CallManagementWindow.ShowSingleton(App.LoggedAdminId);
            });

            OpenVolunteersCommand = new RelayCommand(_ =>
            {
                if (IsSimulatorRunning)
                    new VolunteerListWindow().Show();
                else
                    VolunteerListWindow.ShowSingleton();
            });

            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalMinutes;
            CurrentTime = s_bl.Admin.GetClock();

            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);

            s_bl.Call.AddObserver(_refreshCallQuantitiesObserver);
            LoadCallQuantities();

            DataContext = this;
        }

        private void clockObserver()
        {
            if (_clockObserverOperation is null || _clockObserverOperation.Status == DispatcherOperationStatus.Completed)
            {
                _clockObserverOperation = Dispatcher.BeginInvoke(() =>
                {
                    CurrentTime = s_bl.Admin.GetClock();
                });
            }
        }

        private void _refreshCallQuantitiesObserver()
        {
            if (_refreshCallQuantitiesOperation is null || _refreshCallQuantitiesOperation.Status == DispatcherOperationStatus.Completed)
            {
                _refreshCallQuantitiesOperation = Dispatcher.BeginInvoke(() =>
                {
                    LoadCallQuantities();
                });
            }
        }

        private void configObserver() =>
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalMinutes;

        private void LoadCallQuantities()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => LoadCallQuantities());
                return;
            }

            try
            {
                var quantities = s_bl.Call.GetCallQuantitiesByStatus();
                CallQuantities.Clear();
                foreach (CallStatus status in Enum.GetValues(typeof(CallStatus)))
                {
                    if (status != CallStatus.none)
                    {
                        CallQuantities.Add(new CallQuantity
                        {
                            Status = status,
                            Quantity = quantities[(int)status]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading call quantities: {ex.Message}");
            }
        }

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

        private void ToggleSimulator()
        {
            if (!IsSimulatorRunning)
                StartSimulator();
            else
                StopSimulator();
        }

        private void StartSimulator()
        {
            if (IsSimulatorRunning)
            {
                MessageBox.Show("Simulator is already running.");
                return;
            }
            IsSimulatorRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => RunSimulator(_cancellationTokenSource.Token));
        }

        private void StopSimulator()
        {
            if (!IsSimulatorRunning)
            {
                MessageBox.Show("Simulator is not running.");
                return;
            }
            _cancellationTokenSource?.Cancel();
            IsSimulatorRunning = false;
            s_bl.Admin.StopSimulator();
        }

        private async Task RunSimulator(CancellationToken token)
        {
            try
            {
                int currentInterval = 0;

                await Dispatcher.InvokeAsync(() =>
                {
                    currentInterval = Interval;
                });

                s_bl.Admin.StartSimulator(currentInterval);

                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(1000, token);
                }

                s_bl.Admin.StopSimulator();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                ShowError($"Error in simulator: {ex.Message}");
            }
            finally
            {
                Dispatcher.Invoke(() => IsSimulatorRunning = false);
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

                    dbAction();

                    MessageBox.Show($"Database {actionName.ToLower()}ed successfully.", "Success");
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private void OpenWindow<T>() where T : Window, new()
        {
            if (IsSimulatorRunning)
            {
                new T().Show();
                return;
            }
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
            if (IsSimulatorRunning)
                StopSimulator();
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
            s_bl.Call.RemoveObserver(_refreshCallQuantitiesObserver);

            App.IsAdminLoggedIn = false;
            if (App.LoggedAdminId > 0)
                s_bl.Volunteer.LogoutVolunteer(App.LoggedAdminId);
        }
    }
}