//using BlApi;
//using BO;
//using PL.Call;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Windows;
//using System.Windows.Input;
//using static BO.Enums;


//namespace PL
//{
//    public partial class SelectCallForTreatmentWindow : Window
//    {
//        private static readonly IBl s_bl = Factory.Get();
//        public BO.Volunteer CurrentVolunteer { get; set; }

//        public SelectCallForTreatmentWindow(int volunteerId, string volunteerAddress, double maxDistance, BO.Volunteer currentVolunteer)
//        {
//            InitializeComponent();

//            VolunteerId = volunteerId;
//            VolunteerAddress = volunteerAddress;
//            MaxDistance = maxDistance;
//            CurrentVolunteer = currentVolunteer;

//            SelectCallCommand = new RelayCommand(param => SelectCall(param));
//            UpdateAddressCommand = new RelayCommand(param => UpdateAddress(param));

//            OpenCalls = new ObservableCollection<OpenCallInList>();
//            DataContext = this;

//            QueryOpenCalls();
//        }

//        private void SelectCall(object parameter)
//        {
//            if (parameter is OpenCallInList call)
//            {
//                try
//                {
//                    s_bl.Call.SelectCallForTreatment(volunteerId: VolunteerId, callId: call.Id);

//                    OpenCalls.Remove(call);

//                    MessageBox.Show("הקריאה הוקצתה בהצלחה עבורך!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);

//                     this.Close();
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"שגיאה בבחירת הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
//                }
//            }
//        }


//        //private IEnumerable<OpenCallInList> LoadOpenCalls()
//        //{
//        //    return new List<OpenCallInList>
//        //    {
//        //        new OpenCallInList()
//        //    };
//        //}

//        //private void UpdateCallStatus(int callId, string status)
//        //{
//        //    s_bl.Call.SelectCallForTreatment(volunteerId: VolunteerId, callId: callId);
//        //}

//        public IEnumerable<CallType> CallTypeCollection => Enum.GetValues(typeof(CallType)).Cast<CallType>();
//        public IEnumerable<string> SortOptions => new[] { "Distance", "Type", "ID" };

//        public CallType SelectedCallType { get; set; } = CallType.none;
//        public string SelectedSortOption { get; set; } = "Distance";
//        public string VolunteerAddress { get; set; }

//        public ObservableCollection<OpenCallInList> OpenCalls { get; set; }

//        public ICommand SelectCallCommand { get; }
//        public ICommand UpdateAddressCommand { get; }

//        public int VolunteerId { get; set; }
//        public double MaxDistance { get; set; }

//        private void QueryOpenCalls()
//        {
//            try
//            {
//                // שליפת הקריאות הפתוחות עבור המתנדב דרך BL
//                var calls = s_bl.Call.GetOpenCallsForVolunteer(
//                    VolunteerId,
//                    SelectedCallType == CallType.none ? null : SelectedCallType,
//                    SelectedSortOption == "Distance" ? BO.Enums.OpenCallInListFields.CallDistance :
//                    SelectedSortOption == "Type" ? BO.Enums.OpenCallInListFields.CallType :
//                    BO.Enums.OpenCallInListFields.Id
//                )
//                // סינון לפי מרחק מקסימלי של המתנדב
//                .Where(call => call.CallDistance <= MaxDistance)
//                .ToList();

//                OpenCalls = new ObservableCollection<OpenCallInList>(calls);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"שגיאה בטעינת הקריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }


//        private void UpdateAddress(object parameter)
//        {
//            try
//            {
//                s_bl.Volunteer.UpdateVolunteerDetails(VolunteerId, new BO.Volunteer { FullAddress = VolunteerAddress });
//                QueryOpenCalls(); // עדכון הרשימה לאחר שינוי הכתובת
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"שגיאה בעדכון הכתובת: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void ViewCurrentCallDetails_Click(object sender, RoutedEventArgs e)
//        {
//            if (CurrentVolunteer == null || CurrentVolunteer.CallInProgress == null)
//            {
//                MessageBox.Show("This volunteer has no current call assigned.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
//                return;
//            }

//            try
//            {
//                var currentCall = s_bl.Call.GetCallDetails(CurrentVolunteer.CallInProgress.CallId);

//                var selectCallWindow = new SelectCallForTreatmentWindow(CurrentVolunteer.Id, CurrentVolunteer.FullAddress ?? string.Empty, CurrentVolunteer.MaxDistance.Value, CurrentVolunteer)
//                {
//                    DataContext = currentCall,
//                    IsReadOnly = true
//                };
//                selectCallWindow.ShowDialog();
//            }

//            catch (Exception ex)
//            {
//                MessageBox.Show($"An error occurred while retrieving the call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        public bool IsReadOnly { get; set; }


//    }
//}
using BlApi;
using BO;
using PL.Call;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static BO.Enums;

namespace PL
{
    public partial class SelectCallForTreatmentWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsManager { get; set; }
        public BO.Volunteer CurrentVolunteer { get; set; }

        public SelectCallForTreatmentWindow(int volunteerId, string volunteerAddress, double maxDistance, BO.Volunteer currentVolunteer)
        {
            InitializeComponent();

            VolunteerId = volunteerId;
            VolunteerAddress = volunteerAddress;
            MaxDistance = maxDistance;
            CurrentVolunteer = currentVolunteer;

            SelectCallCommand = new RelayCommand(param => SelectCall(param));
            UpdateAddressCommand = new RelayCommand(param => UpdateAddress(param));

            OpenCalls = new ObservableCollection<OpenCallInList>();
            OpenCallsView = (ListCollectionView)CollectionViewSource.GetDefaultView(OpenCalls);

            DataContext = this;

            

            QueryOpenCalls();
        }

        private void SelectCall(object parameter)
        {
            if (parameter is OpenCallInList call)
            {
                try
                {
                    s_bl.Call.SelectCallForTreatment(volunteerId: VolunteerId, callId: call.Id);

                    OpenCalls.Remove(call);

                    MessageBox.Show("הקריאה הוקצתה בהצלחה עבורך!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בבחירת הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public IEnumerable<CallType> CallTypeCollection => Enum.GetValues(typeof(CallType)).Cast<CallType>();
        public IEnumerable<string> SortOptions => new[] { "Distance", "Type", "ID" };
        public IEnumerable<string> GroupOptions => new[] { "None", "CallType", "FullAddress" };

        private CallType _selectedCallType = CallType.none;
        public CallType SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    OnPropertyChanged(nameof(SelectedCallType));
                    QueryOpenCalls();
                }
            }
        }

        private string _selectedSortOption = "Distance";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged(nameof(SelectedSortOption));
                    QueryOpenCalls();
                }
            }
        }

        private string _selectedGroupOption = "None";
        public string SelectedGroupOption
        {
            get => _selectedGroupOption;
            set
            {
                if (_selectedGroupOption != value)
                {
                    _selectedGroupOption = value;
                    OnPropertyChanged(nameof(SelectedGroupOption));
                    ApplyGrouping();
                }
            }
        }

        private string _addressFilter = string.Empty;
        public string AddressFilter
        {
            get => _addressFilter;
            set
            {
                if (_addressFilter != value)
                {
                    _addressFilter = value;
                    OnPropertyChanged(nameof(AddressFilter));
                    QueryOpenCalls();
                }
            }
        }

        public ObservableCollection<OpenCallInList> OpenCalls { get; set; }
        public ListCollectionView OpenCallsView { get; set; }

        public ICommand SelectCallCommand { get; }
        public ICommand UpdateAddressCommand { get; }

        public int VolunteerId { get; set; }
        public double MaxDistance { get; set; }
        public string VolunteerAddress { get; set; }

        private void QueryOpenCalls()
        {
            try
            {
                var calls = s_bl.Call.GetOpenCallsForVolunteer(
                    VolunteerId,
                    SelectedCallType == CallType.none ? null : SelectedCallType,
                    SelectedSortOption == "Distance" ? BO.Enums.OpenCallInListFields.CallDistance :
                    SelectedSortOption == "Type" ? BO.Enums.OpenCallInListFields.CallType :
                    BO.Enums.OpenCallInListFields.Id
                );

                // סינון לפי כתובת
                if (!string.IsNullOrWhiteSpace(AddressFilter))
                    calls = calls.Where(call => call.FullAddress != null && call.FullAddress.Contains(AddressFilter, StringComparison.OrdinalIgnoreCase));

                // סינון לפי מרחק
                calls = calls.Where(call => call.CallDistance <= MaxDistance);

                // מיון
                calls = SelectedSortOption switch
                {
                    "Distance" => calls.OrderBy(c => c.CallDistance),
                    "Type" => calls.OrderBy(c => c.CallType),
                    "ID" => calls.OrderBy(c => c.Id),
                    _ => calls
                };

                OpenCalls.Clear();
                foreach (var call in calls)
                    OpenCalls.Add(call);

                ApplyGrouping();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הקריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyGrouping()
        {
            OpenCallsView.GroupDescriptions.Clear();
            if (SelectedGroupOption == "CallType")
                OpenCallsView.GroupDescriptions.Add(new PropertyGroupDescription("CallType"));
            else if (SelectedGroupOption == "FullAddress")
                OpenCallsView.GroupDescriptions.Add(new PropertyGroupDescription("FullAddress"));
        }

        private void UpdateAddress(object parameter)
        {
            try
            {
                s_bl.Volunteer.UpdateVolunteerDetails(VolunteerId, new BO.Volunteer { FullAddress = VolunteerAddress });
                QueryOpenCalls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בעדכון הכתובת: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void ViewCurrentCallDetails_Click(object sender, RoutedEventArgs e)
        //{
        //    if (CurrentVolunteer == null || CurrentVolunteer.CallInProgress == null)
        //    {
        //        MessageBox.Show("This volunteer has no current call assigned.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        //        return;
        //    }

        //    try
        //    {
        //        var currentCall = s_bl.Call.GetCallDetails(CurrentVolunteer.CallInProgress.CallId);

        //        var selectCallWindow = new SelectCallForTreatmentWindow(CurrentVolunteer.Id, CurrentVolunteer.FullAddress ?? string.Empty, CurrentVolunteer.MaxDistance.Value, CurrentVolunteer)
        //        {
        //            DataContext = currentCall,
        //            IsReadOnly = true
        //        };
        //        selectCallWindow.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"An error occurred while retrieving the call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        public bool IsReadOnly { get; set; }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}