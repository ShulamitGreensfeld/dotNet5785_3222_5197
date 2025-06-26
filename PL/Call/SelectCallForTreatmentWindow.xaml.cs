using BlApi;
using BO;
using DalApi;
using DO;
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
        private static readonly IBl s_bl = BlApi.Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable<CallType> CallTypeCollection { get; } = Enum.GetValues(typeof(CallType)).Cast<CallType>();
        public BO.Volunteer CurrentVolunteer { get; set; }

        public List<string> SortOptions { get; } = new() { "Distance", "Type", "ID" };
        public List<string> GroupOptions { get; } = new() { "None", "CallType", "FullAddress" };

        public SelectCallForTreatmentWindow(int volunteerId, string volunteerAddress, double maxDistance, BO.Volunteer currentVolunteer)
        {
            InitializeComponent();

            CurrentVolunteer = currentVolunteer;
            VolunteerId = volunteerId;
            VolunteerAddress = currentVolunteer?.FullAddress ?? string.Empty;
            MaxDistance = maxDistance;

            SelectCallCommand = new RelayCommand(param => SelectCall(param));
            UpdateAddressCommand = new RelayCommand(param => UpdateAddress(param));

            OpenCalls = new ObservableCollection<OpenCallInList>();
            OpenCallsView = (ListCollectionView)CollectionViewSource.GetDefaultView(OpenCalls);

            // הגדרת פילטר ומיון על ה-View
            OpenCallsView.Filter = FilterCalls;
            OpenCallsView.SortDescriptions.Add(new SortDescription("CallDistance", ListSortDirection.Ascending));

            DataContext = this;

            QueryOpenCalls();
        }

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
                    OpenCallsView.Refresh();
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
                    ApplySorting();
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
                    OpenCallsView.Refresh();
                }
            }
        }

        private string _descriptionFilter = string.Empty;
        public string DescriptionFilter
        {
            get => _descriptionFilter;
            set
            {
                if (_descriptionFilter != value)
                {
                    _descriptionFilter = value;
                    OnPropertyChanged(nameof(DescriptionFilter));
                    OpenCallsView.Refresh();
                }
            }
        }

        public ObservableCollection<OpenCallInList> OpenCalls { get; set; }
        public ListCollectionView OpenCallsView { get; set; }

        public ICommand SelectCallCommand { get; }
        public ICommand UpdateAddressCommand { get; }

        public int VolunteerId { get; set; }
        public double MaxDistance { get; set; }

        private void QueryOpenCalls()
        {
            try
            {
                var calls = s_bl.Call.GetOpenCallsForVolunteer(
                    VolunteerId,
                    null,
                    null  
                )
                //.Where(call => call.CallDistance <= MaxDistance)
                .ToList();

                OpenCalls.Clear();
                foreach (var call in calls)
                    OpenCalls.Add(call);

                ApplySorting();
                ApplyGrouping();
                OpenCallsView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הקריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool FilterCalls(object obj)
        {
            if (obj is not OpenCallInList call)
                return false;

            // סינון לפי סוג קריאה
            if (SelectedCallType != CallType.none && call.CallType != SelectedCallType)
                return false;

            // סינון לפי כתובת
            if (!string.IsNullOrWhiteSpace(AddressFilter) &&
                (call.FullAddress == null || !call.FullAddress.Contains(AddressFilter, StringComparison.OrdinalIgnoreCase)))
                return false;

            // סינון לפי מרחק (כבר בוצע ב-QueryOpenCalls, אבל אפשר להשאיר ליתר ביטחון)
            if (call.CallDistance > MaxDistance)
                return false;

            return true;
        }

        private void ApplySorting()
        {
            OpenCallsView.SortDescriptions.Clear();
            switch (SelectedSortOption)
            {
                case "Distance":
                    OpenCallsView.SortDescriptions.Add(new SortDescription(nameof(OpenCallInList.CallDistance), ListSortDirection.Ascending));
                    break;
                case "Type":
                    OpenCallsView.SortDescriptions.Add(new SortDescription(nameof(OpenCallInList.CallType), ListSortDirection.Ascending));
                    break;
                case "ID":
                    OpenCallsView.SortDescriptions.Add(new SortDescription(nameof(OpenCallInList.Id), ListSortDirection.Ascending));
                    break;
            }
            OpenCallsView.Refresh();
        }

        private void ApplyGrouping()
        {
            OpenCallsView.GroupDescriptions.Clear();
            if (SelectedGroupOption == "CallType")
                OpenCallsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(OpenCallInList.CallType)));
            else if (SelectedGroupOption == "FullAddress")
                OpenCallsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(OpenCallInList.FullAddress)));
            OpenCallsView.Refresh();
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
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בבחירת הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void UpdateAddress(object parameter)
        {
            try
            {
                if (CurrentVolunteer == null || CurrentVolunteer.Id == 0)
                {
                    MessageBox.Show("Volunteer ID is missing or invalid.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // שליפת כל פרטי המתנדב מה-BL
                var volunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);

                // עדכון כתובת בלבד
                volunteer.FullAddress = VolunteerAddress;

                // נטרול הסיסמה כדי שה-BL לא ידרוש הצפנה/ולידציה
                volunteer.Password = null;

                // שליחת כל האובייקט ל-BL
                s_bl.Volunteer.UpdateVolunteerDetails(volunteer.Id, volunteer);

                // עדכון בזיכרון המקומי
                CurrentVolunteer.FullAddress = VolunteerAddress;

                MessageBox.Show("הכתובת עודכנה בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);

                QueryOpenCalls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בעדכון הכתובת: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _volunteerAddress;
        public string VolunteerAddress
        {
            get => _volunteerAddress;
            set
            {
                if (_volunteerAddress != value)
                {
                    _volunteerAddress = value;
                    OnPropertyChanged(nameof(VolunteerAddress));
                }
            }
        }
        private OpenCallInList _selectedCall;
        public OpenCallInList SelectedCall
        {
            get => _selectedCall;
            set
            {
                if (_selectedCall != value)
                {
                    _selectedCall = value;
                    OnPropertyChanged(nameof(SelectedCall));
                    OnPropertyChanged(nameof(SelectedCallDescription));
                }
            }
        }

        public string SelectedCallDescription => SelectedCall?.Verbal_description ?? string.Empty;
    }
}