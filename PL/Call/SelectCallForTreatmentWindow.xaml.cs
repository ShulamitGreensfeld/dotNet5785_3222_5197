using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BlApi;
using BO;
using static BO.Enums;
using PL.Helpers;


namespace PL
{
    public partial class SelectCallForTreatmentWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get();
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
                    QueryOpenCalls(); // עדכון הרשימה לאחר הבחירה
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"שגיאה בבחירת הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //private IEnumerable<OpenCallInList> LoadOpenCalls()
        //{
        //    return new List<OpenCallInList>
        //    {
        //        new OpenCallInList()
        //    };
        //}

        //private void UpdateCallStatus(int callId, string status)
        //{
        //    s_bl.Call.SelectCallForTreatment(volunteerId: VolunteerId, callId: callId);
        //}

        public IEnumerable<CallType> CallTypeCollection => Enum.GetValues(typeof(CallType)).Cast<CallType>();
        public IEnumerable<string> SortOptions => new[] { "Distance", "Type", "ID" };

        public CallType SelectedCallType { get; set; } = CallType.none;
        public string SelectedSortOption { get; set; } = "Distance";
        public string VolunteerAddress { get; set; }

        public ObservableCollection<OpenCallInList> OpenCalls { get; set; }

        public ICommand SelectCallCommand { get; }
        public ICommand UpdateAddressCommand { get; }

        public int VolunteerId { get; set; }
        public double MaxDistance { get; set; }

        private void QueryOpenCalls()
        {
            try
            {
                var calls = s_bl.Call.GetCallsList(null, null, null)
                    .Where(call => call.CallStatus == CallStatus.opened || call.CallStatus == CallStatus.opened_at_risk)
                    .Select(call => new OpenCallInList
                    {
                        Id = call.CallId,
                        CallType = call.CallType,
                        //FullAddress = call.FullAddress,
                        Start_time = call.Opening_time,
                        //Max_finish_time = call.Max_finish_time,
                        CallDistance = ToolsProxy.CalculateDistance(
                            CurrentVolunteer.Latitude ?? 0,
                            CurrentVolunteer.Longitude ?? 0,
                            0, 
                            0
                        )
                    }).ToList();

                OpenCalls = new ObservableCollection<OpenCallInList>(calls);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת הקריאות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateAddress(object parameter)
        {
            try
            {
                s_bl.Volunteer.UpdateVolunteerDetails(VolunteerId, new BO.Volunteer { FullAddress = VolunteerAddress });
                QueryOpenCalls(); // עדכון הרשימה לאחר שינוי הכתובת
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בעדכון הכתובת: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewCurrentCallDetails_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer == null || CurrentVolunteer.CallInProgress == null)
            {
                MessageBox.Show("This volunteer has no current call assigned.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var currentCall = s_bl.Call.GetCallDetails(CurrentVolunteer.CallInProgress.CallId);

                var selectCallWindow = new SelectCallForTreatmentWindow(CurrentVolunteer.Id, CurrentVolunteer.FullAddress ?? string.Empty, CurrentVolunteer.MaxDistance ?? 0, CurrentVolunteer)
                {
                    DataContext = currentCall,
                    IsReadOnly = true
                };
                selectCallWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while retrieving the call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool IsReadOnly { get; set; }
    }
}