using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using BlApi;
using BO;
using static BO.Enums;

namespace PL
{
    public partial class VolunteerCallHistoryWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public VolunteerCallHistoryWindow(int volunteerId)
        {
            InitializeComponent();
            VolunteerId = volunteerId;
            SelectedCallType = CallType.none;
            SelectedSortField = ClosedCallInListFields.Id;
            DataContext = this;
            LoadClosedCalls();
        }

        // Volunteer ID
        public int VolunteerId { get; set; }

        // List of call types
        public IEnumerable<CallType> CallTypeCollection => System.Enum.GetValues(typeof(CallType)) as CallType[];

        // List of sort fields
        public IEnumerable<ClosedCallInListFields> SortFields => System.Enum.GetValues(typeof(ClosedCallInListFields)) as ClosedCallInListFields[];

        // Selected call type filter
        private CallType _selectedCallType;
        public CallType SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                _selectedCallType = value;
                OnPropertyChanged();
                LoadClosedCalls();
            }
        }

        // Selected sort field
        private ClosedCallInListFields _selectedSortField;
        public ClosedCallInListFields SelectedSortField
        {
            get => _selectedSortField;
            set
            {
                _selectedSortField = value;
                OnPropertyChanged();
                LoadClosedCalls();
            }
        }

        // List of closed calls
        private IEnumerable<ClosedCallInList> _closedCalls = new List<ClosedCallInList>();
        public IEnumerable<ClosedCallInList> ClosedCalls
        {
            get => _closedCalls;
            set
            {
                _closedCalls = value;
                OnPropertyChanged();
            }
        }

        // Load closed calls from BL based on filters
        private void LoadClosedCalls()
        {
            ClosedCalls = s_bl.Call.GetClosedCallsHandledByVolunteer(
                volunteerId: VolunteerId,
                callTypeFilter: SelectedCallType == CallType.none ? null : SelectedCallType,
                sortField: SelectedSortField
            );
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}