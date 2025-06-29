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

namespace PL
{
    public partial class CallManagementWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        // Commands for UI buttons
        public ICommand DeleteCommand { get; }
        public ICommand CancelAssignmentCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand OpenCallDetailsCommand { get; }

        // Observer pattern support
        private readonly Action _refreshCallQuantities;
        private readonly Dictionary<int, Action> _callObservers = new();

        // Array of call quantities by status
        private int[] _callQuantities;
        public int[] CallQuantities
        {
            get => _callQuantities;
            set
            {
                _callQuantities = value;
                OnPropertyChanged(nameof(CallQuantities));
            }
        }

        public CallManagementWindow() : this(null) { }

        private void OpenCallDetails()
        {
            if (SelectedCall == null)
                return;

            try
            {
                var window = new SingleCallWindow(SelectedCall.CallId)
                {
                    Owner = this
                };
                window.ShowDialog(); 
                UpdateCallList();   
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening call details: " + ex.Message);
            }
        }

        public CallManagementWindow(BO.Enums.CallStatus? initialStatus = null)
        {
            InitializeComponent();

            // Bind commands to logic
            DeleteCommand = new RelayCommand(param => DeleteCall(param));
            CancelAssignmentCommand = new RelayCommand(param => CancelAssignment(param));
            AddCommand = new RelayCommand(_ => AddCall());

            _refreshCallQuantities = RefreshCallQuantities;
            s_bl.Call.AddObserver(_refreshCallQuantities);

            DataContext = this;

            SelectedCallStatus = initialStatus ?? BO.Enums.CallStatus.none;
            OpenCallDetailsCommand = new RelayCommand(_ => OpenSelectedCallWindow(), _ => SelectedCall != null);

            UpdateCallList();
            RefreshCallQuantities();
        }

        // Collections for filtering/grouping options
        public IEnumerable<BO.Enums.CallType> CallTypeCollection =>
            Enum.GetValues(typeof(BO.Enums.CallType)).Cast<BO.Enums.CallType>();

        public IEnumerable<BO.Enums.CallStatus> CallStatusCollection =>
            Enum.GetValues(typeof(BO.Enums.CallStatus)).Cast<BO.Enums.CallStatus>();

        public IEnumerable<string> SortFields => new[] { "Opening_time", "CallType", "CallStatus", "TotalAssignments" };
        public IEnumerable<string> GroupFields => new[] { "None", "CallType", "CallStatus" };

        // Selected filter/sort/group fields
        private BO.Enums.CallType _selectedCallType = BO.Enums.CallType.none;
        public BO.Enums.CallType SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    OnPropertyChanged(nameof(SelectedCallType));
                    UpdateCallList();
                }
            }
        }

        private BO.Enums.CallStatus? _selectedCallStatus = BO.Enums.CallStatus.none;
        public BO.Enums.CallStatus? SelectedCallStatus
        {
            get => _selectedCallStatus;
            set
            {
                if (_selectedCallStatus != value)
                {
                    _selectedCallStatus = value;
                    OnPropertyChanged(nameof(SelectedCallStatus));
                    UpdateCallList();
                }
            }
        }

        private void OpenSelectedCallWindow()
        {
            if (SelectedCall == null)
                return;

            try
            {
                var window = new PL.Call.SingleCallWindow(SelectedCall.CallId);
                window.Show();
                UpdateCallList(); // Refresh list after
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening call details: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string _selectedSortField = "Opening_time";
        public string SelectedSortField
        {
            get => _selectedSortField;
            set
            {
                if (_selectedSortField != value)
                {
                    _selectedSortField = value;
                    OnPropertyChanged(nameof(SelectedSortField));
                    UpdateCallList();
                }
            }
        }

        private string _selectedGroupField = "None";
        public string SelectedGroupField
        {
            get => _selectedGroupField;
            set
            {
                if (_selectedGroupField != value)
                {
                    _selectedGroupField = value;
                    OnPropertyChanged(nameof(SelectedGroupField));
                    UpdateCallList();
                }
            }
        }

        // Main data list
        private IEnumerable<CallInList> _callList;
        public IEnumerable<CallInList> CallList
        {
            get => _callList;
            set
            {
                _callList = value;
                OnPropertyChanged(nameof(CallList));
            }
        }

        // CollectionView for grouping
        private ListCollectionView _callListView;
        public ListCollectionView CallListView
        {
            get => _callListView;
            set
            {
                _callListView = value;
                OnPropertyChanged(nameof(CallListView));
            }
        }

        public CallInList? SelectedCall { get; set; }

        /// <summary>
        /// Updates the displayed list based on filters and sort options.
        /// Registers observers for individual calls.
        /// </summary>
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

            // Remove previous observers
            foreach (var kvp in _callObservers)
                s_bl.Call.RemoveObserver(kvp.Key, kvp.Value);
            _callObservers.Clear();

            // Register new observers per call
            foreach (var call in CallList)
            {
                int callId = call.CallId;
                Action observer = () => Dispatcher.Invoke(UpdateCallList);
                _callObservers[callId] = observer;
                s_bl.Call.AddObserver(callId, observer);
            }

            // Setup grouping
            var lcv = new ListCollectionView(CallList.ToList());
            lcv.GroupDescriptions.Clear();

            if (SelectedGroupField == "CallType")
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("CallType"));
            else if (SelectedGroupField == "CallStatus")
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("CallStatus"));

            CallListView = lcv;
        }

        /// <summary>
        /// Refreshes call counts by status.
        /// </summary>
        private void RefreshCallQuantities()
        {
            CallQuantities = s_bl.Call.GetCallQuantitiesByStatus();
        }

        /// <summary>
        /// Cleans up all observers when window closes.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            foreach (var kvp in _callObservers)
                s_bl.Call.RemoveObserver(kvp.Key, kvp.Value);
            _callObservers.Clear();

            s_bl.Call.RemoveObserver(_refreshCallQuantities);
            App.IsAdminLoggedIn = false;
        }

        /// <summary>
        /// Deletes a call only if it's open and has no assignments.
        /// </summary>
        private void DeleteCall(object param)
        {
            if (param is not CallInList call) return;

            if (call.CallStatus != BO.Enums.CallStatus.opened || call.TotalAssignments > 0)
            {
                MessageBox.Show("Only open calls with no assignments can be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this call?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
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
        }

        /// <summary>
        /// Cancels a volunteer assignment to an ongoing call.
        /// </summary>
        private void CancelAssignment(object param)
        {
            if (param is not CallInList call) return;

            if (call.CallStatus != BO.Enums.CallStatus.is_treated || string.IsNullOrEmpty(call.LastVolunteerName))
            {
                MessageBox.Show("Only calls with status 'in treatment' and an assigned volunteer can be canceled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int? currentUserId = App.CurrentUserId;
            int? assignmentId = call.Id;

            if (currentUserId == null || assignmentId == null)
            {
                MessageBox.Show("Missing user ID or call ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Cancel the assignment for this call? A notification will be sent to the volunteer.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Call.MarkCallCancellation(currentUserId.Value, assignmentId.Value);
                    UpdateCallList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cancelling assignment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Opens the Add Call window.
        /// </summary>
        private void AddCall()
        {
            try
            {
                var addCallWindow = new AddCallWindow { Owner = this };
                addCallWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening window: " + ex.Message);
            }
        }

        /// <summary>
        /// Opens the SingleCallWindow when double-clicking a call row.
        /// </summary>
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCall != null)
            {
                try
                {
                    var window = new SingleCallWindow(SelectedCall.CallId);
                    window.Owner = this;
                    window.Show();
                    UpdateCallList(); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening call details: " + ex.Message);
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}