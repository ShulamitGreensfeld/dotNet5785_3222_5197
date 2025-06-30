using BlApi;
using BO;
using PL.Call;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using PL.Commands;

namespace PL
{
    public partial class CallManagementWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public ICommand DeleteCommand { get; }
        public ICommand CancelAssignmentCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand OpenCallDetailsCommand { get; }

        private readonly Action _refreshCallQuantities;
        private readonly Dictionary<int, Action> _callObservers = new();
        private readonly int _currentUserId;

        private int[] _callQuantities;
        public int[] CallQuantities
        {
            get => _callQuantities;
            set { _callQuantities = value; OnPropertyChanged(nameof(CallQuantities)); }
        }

        public CallManagementWindow() : this(-1, null) { }

        public CallManagementWindow(int currentUserId, BO.Enums.CallStatus? initialStatus = null)
        {
            InitializeComponent();

            _currentUserId = currentUserId;
            DeleteCommand = new RelayCommand(param => ExecuteDeleteCommand(param));
            CancelAssignmentCommand = new RelayCommand(param => ExecuteCancelAssignmentCommand(param));
            AddCommand = new RelayCommand(_ => AddCall());

            _refreshCallQuantities = RefreshCallQuantities;
            s_bl.Call.AddObserver(_refreshCallQuantities);

            DataContext = this;

            SelectedCallStatus = initialStatus ?? BO.Enums.CallStatus.none;
            OpenCallDetailsCommand = new RelayCommand(_ => OpenSelectedCallWindow(), _ => SelectedCall != null);

            UpdateCallList();
            RefreshCallQuantities();
        }

        public IEnumerable<BO.Enums.CallType> CallTypeCollection => Enum.GetValues(typeof(BO.Enums.CallType)).Cast<BO.Enums.CallType>();
        public IEnumerable<BO.Enums.CallStatus> CallStatusCollection => Enum.GetValues(typeof(BO.Enums.CallStatus)).Cast<BO.Enums.CallStatus>();
        public IEnumerable<string> SortFields => new[] { "Opening_time", "CallType", "CallStatus", "TotalAssignments" };
        public IEnumerable<string> GroupFields => new[] { "None", "CallType", "CallStatus" };

        private BO.Enums.CallType _selectedCallType = BO.Enums.CallType.none;
        public BO.Enums.CallType SelectedCallType
        {
            get => _selectedCallType;
            set { _selectedCallType = value; OnPropertyChanged(nameof(SelectedCallType)); UpdateCallList(); }
        }

        private BO.Enums.CallStatus? _selectedCallStatus = BO.Enums.CallStatus.none;
        public BO.Enums.CallStatus? SelectedCallStatus
        {
            get => _selectedCallStatus;
            set { _selectedCallStatus = value; OnPropertyChanged(nameof(SelectedCallStatus)); UpdateCallList(); }
        }

        private string _selectedSortField = "Opening_time";
        public string SelectedSortField
        {
            get => _selectedSortField;
            set { _selectedSortField = value; OnPropertyChanged(nameof(SelectedSortField)); UpdateCallList(); }
        }

        private string _selectedGroupField = "None";
        public string SelectedGroupField
        {
            get => _selectedGroupField;
            set { _selectedGroupField = value; OnPropertyChanged(nameof(SelectedGroupField)); UpdateCallList(); }
        }

        private IEnumerable<CallInList> _callList;
        public IEnumerable<CallInList> CallList
        {
            get => _callList;
            set { _callList = value; OnPropertyChanged(nameof(CallList)); }
        }

        private ListCollectionView _callListView;
        public ListCollectionView CallListView
        {
            get => _callListView;
            set { _callListView = value; OnPropertyChanged(nameof(CallListView)); }
        }

        public CallInList? SelectedCall { get; set; }

        private void UpdateCallList()
        {
            BO.Enums.CallInListFields? filterField = null;
            object? filterValue = null;

            if (SelectedCallType != BO.Enums.CallType.none)
            {
                filterField = BO.Enums.CallInListFields.CallType;
                filterValue = SelectedCallType;
            }
            else if (SelectedCallStatus != BO.Enums.CallStatus.none)
            {
                filterField = BO.Enums.CallInListFields.CallStatus;
                filterValue = SelectedCallStatus;
            }

            BO.Enums.CallInListFields? sortField = SelectedSortField switch
            {
                "CallType" => BO.Enums.CallInListFields.CallType,
                "CallStatus" => BO.Enums.CallInListFields.CallStatus,
                "TotalAssignments" => BO.Enums.CallInListFields.TotalAssignments,
                _ => BO.Enums.CallInListFields.Opening_time
            };

            var list = s_bl.Call.GetCallsList(filterField, filterValue, sortField);
            CallList = list.ToList();

            foreach (var kvp in _callObservers)
                s_bl.Call.RemoveObserver(kvp.Key, kvp.Value);
            _callObservers.Clear();

            foreach (var call in CallList)
            {
                int callId = call.CallId;
                Action observer = () => Dispatcher.Invoke(UpdateCallList);
                _callObservers[callId] = observer;
                s_bl.Call.AddObserver(callId, observer);
            }

            var lcv = new ListCollectionView(CallList.ToList());
            lcv.GroupDescriptions.Clear();

            if (SelectedGroupField == "CallType")
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("CallType"));
            else if (SelectedGroupField == "CallStatus")
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("CallStatus"));

            CallListView = lcv;
        }

        private void RefreshCallQuantities() => CallQuantities = s_bl.Call.GetCallQuantitiesByStatus();

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            foreach (var kvp in _callObservers)
                s_bl.Call.RemoveObserver(kvp.Key, kvp.Value);
            _callObservers.Clear();
            s_bl.Call.RemoveObserver(_refreshCallQuantities);
            App.IsAdminLoggedIn = false;

        }

        private void ExecuteDeleteCommand(object param)
        {
            if (param is not CallInList call) return;
            if (MessageBox.Show("Are you sure you want to delete this call?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                s_bl.Call.DeleteCall(call.CallId);
                UpdateCallList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelAssignmentCommand(object param)
        {
            if (param is not CallInList call) return;
            if (MessageBox.Show("Cancel the assignment for this call? A notification will be sent to the volunteer.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                if (call.Id.HasValue)
                {
                    s_bl.Call.MarkCallCancellation(_currentUserId, call.Id.Value);
                    UpdateCallList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancelling assignment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCall()
        {
            try
            {
                var addCallWindow = new AddCallWindow { Owner = this };
                addCallWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening window: " + ex.Message);
            }
        }

        private void OpenSelectedCallWindow()
        {
            if (SelectedCall == null) return;

            try
            {
                var window = new SingleCallWindow(SelectedCall.CallId);
                window.Owner = this;
                window.Show();
                UpdateCallList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening call details: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCall == null) return;
            OpenSelectedCallWindow();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}