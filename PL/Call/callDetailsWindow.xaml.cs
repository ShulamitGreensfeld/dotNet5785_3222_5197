//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Windows;
//using BlApi;
//using BO;

//namespace PL.Call
//{
//    /// <summary>
//    /// A window for viewing or editing a single call's details.
//    /// Supports both Add and Update modes, fully MVVM-compliant.
//    /// </summary>
//    public partial class CallDetailsWindow : Window, INotifyPropertyChanged
//    {
//        private static readonly IBl s_bl = BlApi.Factory.Get();

//        public event PropertyChangedEventHandler? PropertyChanged;


//        /// <summary>
//        /// Notifies UI that a property has changed
//        /// </summary>
//        private void OnPropertyChanged(string propertyName) =>
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

//        /// <summary>
//        /// Indicates whether the window is in "Add" mode (as opposed to Edit/View)
//        /// </summary>
//        public bool IsAddMode { get; }

//        /// <summary>
//        /// Dynamic window title
//        /// </summary>
//        public string WindowTitle => IsAddMode ? "Add Call" : "Call Details";

//        /// <summary>
//        /// Dynamic action button text based on mode
//        /// </summary>
//        public string ActionButtonText => IsAddMode ? "Add" : "Update";

//        /// <summary>
//        /// Indicates whether the action button is enabled
//        /// </summary>
//        public bool IsActionButtonEnabled => IsAddMode || (!IsReadOnly &&
//            (Call.CallStatus == BO.Enums.CallStatus.opened ||
//             Call.CallStatus == BO.Enums.CallStatus.opened_at_risk ||
//             Call.CallStatus == BO.Enums.CallStatus.is_treated ||
//             Call.CallStatus == BO.Enums.CallStatus.treated_at_risk));

//        private string? _errorMessage;
//        /// <summary>
//        /// Error message to display in UI
//        /// </summary>
//        public string? ErrorMessage
//        {
//            get => _errorMessage;
//            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
//        }

//        private BO.Call _call = new();
//        /// <summary>
//        /// The call entity being edited or created
//        /// </summary>
//        public BO.Call Call
//        {
//            get => _call;
//            set { _call = value; OnPropertyChanged(nameof(Call)); }
//        }

//        /// <summary>
//        /// List of possible call types for the dropdown
//        /// </summary>
//        public IEnumerable<BO.Enums.CallType> CallTypes =>
//            Enum.GetValues(typeof(BO.Enums.CallType)) as BO.Enums.CallType[];

//        private double _callDistance;
//        /// <summary>
//        /// Distance from the volunteer to the call
//        /// </summary>
//        public double CallDistance
//        {
//            get => _callDistance;
//            set { _callDistance = value; OnPropertyChanged(nameof(CallDistance)); }
//        }

//        private bool _isReadOnly;
//        /// <summary>
//        /// Whether the UI is in read-only mode
//        /// </summary>
//        public bool IsReadOnly
//        {
//            get => _isReadOnly;
//            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
//        }

//        /// <summary>
//        /// Indicates if the call type field is editable
//        /// </summary>
//        public bool IsCallTypeEditable =>
//            IsAddMode || (!IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk));

//        /// <summary>
//        /// Indicates if the description field is editable
//        /// </summary>
//        public bool IsDescriptionReadOnly =>
//            !IsAddMode && (IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk));

//        /// <summary>
//        /// Indicates if the address field is editable
//        /// </summary>
//        public bool IsAddressReadOnly =>
//            !IsAddMode && (IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk));

//        /// <summary>
//        /// Indicates if the max finish time field is editable
//        /// </summary>
//        public bool IsMaxFinishTimeEditable =>
//            IsAddMode || (!IsReadOnly &&
//            (Call.CallStatus == BO.Enums.CallStatus.opened ||
//             Call.CallStatus == BO.Enums.CallStatus.opened_at_risk ||
//             Call.CallStatus == BO.Enums.CallStatus.is_treated ||
//             Call.CallStatus == BO.Enums.CallStatus.treated_at_risk));

//        /// <summary>
//        /// Constructor for Add mode
//        /// </summary>
//        public CallDetailsWindow()
//        {
//            InitializeComponent();
//            IsAddMode = true;
//            Call = new BO.Call
//            {
//                CallType = BO.Enums.CallType.ToPrepareFood,
//                Verbal_description = "",
//                FullAddress = "",
//                Opening_time = s_bl.Admin.GetClock(),
//                Max_finish_time = s_bl.Admin.GetClock(),
//                CallStatus = BO.Enums.CallStatus.opened,
//                AssignmentsList = new List<BO.CallAssignInList>()
//            };
//            IsReadOnly = false;
//            CallDistance = 0;
//            DataContext = this;
//        }

//        /// <summary>
//        /// Constructor for View/Edit mode
//        /// </summary>
//        public CallDetailsWindow(int callId)
//        {
//            InitializeComponent();
//            IsAddMode = false;
//            Call = s_bl.Call.GetCallDetails(callId);
//            CallDistance = 0;
//            IsReadOnly = false;
//            DataContext = this;
//        }

//        /// <summary>
//        /// Handles the Add or Update button click
//        /// </summary>
//        private void AddOrUpdateButton_Click(object sender, RoutedEventArgs e)
//        {
//            ErrorMessage = null;

//            // Basic format validation (allowed in UI layer)
//            if (string.IsNullOrWhiteSpace(Call.FullAddress))
//            {
//                ErrorMessage = "Address is required.";
//                return;
//            }

//            if (Call.Max_finish_time < Call.Opening_time)
//            {
//                ErrorMessage = "Max finish time must be after opening time.";
//                return;
//            }

//            if (IsAddMode)
//            {
//                try
//                {
//                    s_bl.Call.AddCall(Call);
//                    MessageBox.Show("Call added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
//                    Close();
//                }
//                catch (Exception ex)
//                {
//                    ErrorMessage = $"Error adding call: {ex.Message}";
//                }
//            }
//            else
//            {
//                if (IsReadOnly) return;

//                try
//                {
//                    s_bl.Call.UpdateCallDetails(Call);
//                    MessageBox.Show("Call updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
//                    Close();
//                }
//                catch (Exception ex)
//                {
//                    ErrorMessage = $"Error updating call: {ex.Message}";
//                }
//            }
//        }

//        /// <summary>
//        /// Handles the Cancel button click
//        /// </summary>
//        private void CancelButton_Click(object sender, RoutedEventArgs e)
//        {
//            DialogResult = false;
//            Close();
//        }
//        private int? _observedVolunteerId;
//        private Action? _volunteerObserver;

//        public CallDetailsWindow(int callId, int? volunteerId = null)
//        {
//            InitializeComponent();
//            IsAddMode = false;
//            Call = s_bl.Call.GetCallDetails(callId);
//            CallDistance = 0;
//            IsReadOnly = false;
//            DataContext = this;

//            if (volunteerId.HasValue)
//            {
//                _observedVolunteerId = volunteerId;
//                _volunteerObserver = () =>
//                {
//                    var volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId.Value);
//                    if (volunteer.CallInProgress == null || volunteer.CallInProgress.Id != callId)
//                    {
//                        Dispatcher.Invoke(() =>
//                        {
//                            Close();
//                        });
//                    }
//                };
//                s_bl.Volunteer.AddObserver(volunteerId.Value, _volunteerObserver);
//            }
//        }

//        protected override void OnClosed(EventArgs e)
//        {
//            base.OnClosed(e);
//            if (_observedVolunteerId.HasValue && _volunteerObserver != null)
//                s_bl.Volunteer.RemoveObserver(_observedVolunteerId.Value, _volunteerObserver);
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using BlApi;
using BO;

namespace PL.Call
{
    /// <summary>
    /// A window for viewing or editing a single call's details.
    /// Supports both Add and Update modes, fully MVVM-compliant.
    /// </summary>
    public partial class CallDetailsWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;
        private volatile DispatcherOperation? _observerOperation = null; 

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool IsAddMode { get; }
        public string WindowTitle => IsAddMode ? "Add Call" : "Call Details";
        public string ActionButtonText => IsAddMode ? "Add" : "Update";
        public bool IsActionButtonEnabled => IsAddMode || (!IsReadOnly &&
            (Call.CallStatus == BO.Enums.CallStatus.opened ||
             Call.CallStatus == BO.Enums.CallStatus.opened_at_risk ||
             Call.CallStatus == BO.Enums.CallStatus.is_treated ||
             Call.CallStatus == BO.Enums.CallStatus.treated_at_risk));

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        private BO.Call _call = new();
        public BO.Call Call
        {
            get => _call;
            set { _call = value; OnPropertyChanged(nameof(Call)); }
        }

        public IEnumerable<BO.Enums.CallType> CallTypes =>
            Enum.GetValues(typeof(BO.Enums.CallType)) as BO.Enums.CallType[];

        private double _callDistance;
        public double CallDistance
        {
            get => _callDistance;
            set { _callDistance = value; OnPropertyChanged(nameof(CallDistance)); }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
        }

        public bool IsCallTypeEditable =>
            IsAddMode || (!IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk));
        public bool IsDescriptionReadOnly =>
            !IsAddMode && (IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk));
        public bool IsAddressReadOnly =>
            !IsAddMode && (IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk));
        public bool IsMaxFinishTimeEditable =>
            IsAddMode || (!IsReadOnly &&
            (Call.CallStatus == BO.Enums.CallStatus.opened ||
             Call.CallStatus == BO.Enums.CallStatus.opened_at_risk ||
             Call.CallStatus == BO.Enums.CallStatus.is_treated ||
             Call.CallStatus == BO.Enums.CallStatus.treated_at_risk));

        public CallDetailsWindow()
        {
            InitializeComponent();
            IsAddMode = true;
            Call = new BO.Call
            {
                CallType = BO.Enums.CallType.ToPrepareFood,
                Verbal_description = "",
                FullAddress = "",
                Opening_time = s_bl.Admin.GetClock(),
                Max_finish_time = s_bl.Admin.GetClock(),
                CallStatus = BO.Enums.CallStatus.opened,
                AssignmentsList = new List<BO.CallAssignInList>()
            };
            IsReadOnly = false;
            CallDistance = 0;
            DataContext = this;
        }

        public CallDetailsWindow(int callId)
        {
            InitializeComponent();
            IsAddMode = false;
            Call = s_bl.Call.GetCallDetails(callId);
            CallDistance = 0;
            IsReadOnly = false;
            DataContext = this;
            RegisterCallObserver(callId); 
        }

        /// <summary>
        /// Registers observer for updates on the current call (for live updates with simulator/BL changes).
        /// </summary>
        private void RegisterCallObserver(int callId)
        {
            Action observer = CallObserver; 
            s_bl.Call.AddObserver(callId, observer);

            // מסירים את המשקיף בסגירת החלון
            this.Closed += (s, e) => s_bl.Call.RemoveObserver(callId, observer);
        }

        /// <summary>
        /// Observer method to refresh call details on update, via DispatcherOperation (option 2).
        /// </summary>
        private void CallObserver()
        {
            if (_observerOperation is null || _observerOperation.Status == DispatcherOperationStatus.Completed)
            {
                _observerOperation = Dispatcher.BeginInvoke(() =>
                {
                    int id = Call.Id;
                    Call = s_bl.Call.GetCallDetails(id);
                });
            }
        }


        private void AddOrUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Call.FullAddress))
            {
                ErrorMessage = "Address is required.";
                return;
            }

            if (Call.Max_finish_time < Call.Opening_time)
            {
                ErrorMessage = "Max finish time must be after opening time.";
                return;
            }

            if (IsAddMode)
            {
                try
                {
                    s_bl.Call.AddCall(Call);
                    MessageBox.Show("Call added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error adding call: {ex.Message}";
                }
            }
            else
            {
                if (IsReadOnly) return;

                try
                {
                    s_bl.Call.UpdateCallDetails(Call);
                    MessageBox.Show("Call updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error updating call: {ex.Message}";
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private int? _observedVolunteerId;
        private Action? _volunteerObserver;

        public CallDetailsWindow(int callId, int? volunteerId = null)
            : this(callId)
        {
            if (volunteerId.HasValue)
            {
                _observedVolunteerId = volunteerId;
                _volunteerObserver = () =>
                {
                    var volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId.Value);
                    if (volunteer.CallInProgress == null || volunteer.CallInProgress.Id != callId)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Close();
                        });
                    }
                };
                s_bl.Volunteer.AddObserver(volunteerId.Value, _volunteerObserver);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_observedVolunteerId.HasValue && _volunteerObserver != null)
                s_bl.Volunteer.RemoveObserver(_observedVolunteerId.Value, _volunteerObserver);
        }
    }
}
