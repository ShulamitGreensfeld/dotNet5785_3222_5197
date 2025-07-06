using BlApi;
using BO;
using PL.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using static BO.Enums;

namespace PL
{
    public partial class SelectCallForTreatmentWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable<CallType> CallTypeCollection { get; } = Enum.GetValues(typeof(CallType)).Cast<CallType>();
        public List<string> SortOptions { get; } = new() { "Distance", "Type", "ID" };

        public int VolunteerId { get; set; }
        public double MaxDistance { get; set; }

        public BO.Volunteer CurrentVolunteer { get; set; }

        public ICommand SelectCallCommand { get; }
        public ICommand UpdateAddressCommand { get; }

        public ObservableCollection<OpenCallInList> OpenCalls { get; set; } = new();
        public ListCollectionView OpenCallsView { get; set; }

        private volatile DispatcherOperation? _observerOperation = null;
        private readonly Action _callListObserver;

        private CallType _selectedCallType = CallType.none;
        public CallType SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                _selectedCallType = value;
                OnPropertyChanged(nameof(SelectedCallType));
                QueryOpenCalls();
            }
        }

        private string _selectedSortOption = "Distance";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged(nameof(SelectedSortOption));
                QueryOpenCalls();
            }
        }

        private string _addressFilter = string.Empty;
        public string AddressFilter
        {
            get => _addressFilter;
            set
            {
                _addressFilter = value;
                OnPropertyChanged(nameof(AddressFilter));
                QueryOpenCalls();
            }
        }

        private string _descriptionFilter = string.Empty;
        public string DescriptionFilter
        {
            get => _descriptionFilter;
            set
            {
                _descriptionFilter = value;
                OnPropertyChanged(nameof(DescriptionFilter));
                QueryOpenCalls();
            }
        }

        public SelectCallForTreatmentWindow(int volunteerId, string volunteerAddress, double maxDistance, BO.Volunteer currentVolunteer)
        {
            InitializeComponent();

            VolunteerId = volunteerId;
            MaxDistance = maxDistance;
            CurrentVolunteer = currentVolunteer;
            VolunteerAddress = currentVolunteer.FullAddress;

            SelectCallCommand = new RelayCommand(param => SelectCall(param));
            UpdateAddressCommand = new RelayCommand(param => UpdateAddress(param));

            OpenCallsView = (ListCollectionView)CollectionViewSource.GetDefaultView(OpenCalls);
            DataContext = this;

            _callListObserver = CallListObserver;
            s_bl.Call.AddObserver(_callListObserver);

            QueryOpenCalls();
        }

        /// <summary>
        /// מתודת השקפה (observer) שמעדכנת את התצוגה דרך DispatcherOperation
        /// </summary>
        private void CallListObserver()
        {
            if (_observerOperation is null || _observerOperation.Status == DispatcherOperationStatus.Completed)
            {
                _observerOperation = Dispatcher.BeginInvoke((Action)(() => QueryOpenCalls()));
            }
        }

        private void QueryOpenCalls()
        {
            try
            {
                var result = s_bl.Call.GetOpenCallsForVolunteerAsync(
                    VolunteerId,
                    SelectedCallType != CallType.none ? SelectedCallType : null,
                    SelectedSortOption switch
                    {
                        "Type" => BO.Enums.OpenCallInListFields.CallType,
                        "Distance" => BO.Enums.OpenCallInListFields.CallDistance,
                        "ID" => BO.Enums.OpenCallInListFields.Id,
                        _ => null
                    }
                ).GetAwaiter().GetResult()
                .Where(c =>
                    (string.IsNullOrWhiteSpace(AddressFilter) || c.FullAddress?.Contains(AddressFilter, StringComparison.OrdinalIgnoreCase) == true) &&
                    (string.IsNullOrWhiteSpace(DescriptionFilter) || c.Verbal_description?.Contains(DescriptionFilter, StringComparison.OrdinalIgnoreCase) == true) &&
                    (c.CallDistance <= MaxDistance)
                )
                .ToList();

                OpenCalls.Clear();
                foreach (var call in result)
                    OpenCalls.Add(call);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectCall(object parameter)
        {
            if (parameter is OpenCallInList call)
            {
                try
                {
                    s_bl.Call.SelectCallForTreatment(volunteerId: VolunteerId, callId: call.Id);
                    OpenCalls.Remove(call);
                    MessageBox.Show("The call has been successfully assigned for you!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error selecting call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateAddress(object parameter)
        {
            try
            {
                var volunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                volunteer.FullAddress = VolunteerAddress;
                volunteer.Password = null;
                s_bl.Volunteer.UpdateVolunteerDetails(volunteer.Id, volunteer);
                CurrentVolunteer.FullAddress = VolunteerAddress;
                MessageBox.Show("Address updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                QueryOpenCalls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating address: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string _volunteerAddress;
        public string VolunteerAddress
        {
            get => _volunteerAddress;
            set
            {
                _volunteerAddress = value;
                OnPropertyChanged(nameof(VolunteerAddress));
            }
        }

        private OpenCallInList _selectedCall;
        public OpenCallInList SelectedCall
        {
            get => _selectedCall;
            set
            {
                _selectedCall = value;
                OnPropertyChanged(nameof(SelectedCall));
                OnPropertyChanged(nameof(SelectedCallDescription));
            }
        }

        public string SelectedCallDescription => SelectedCall?.Verbal_description ?? string.Empty;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// הסרת המשקיף בעת סגירת המסך!
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            s_bl.Call.RemoveObserver(_callListObserver);
        }
    }
}
