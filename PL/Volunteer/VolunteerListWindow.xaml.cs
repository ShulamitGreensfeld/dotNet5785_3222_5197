using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window
    {
        // Static reference to the BL layer
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // Property for the selected call type (used for filtering)
        public BO.Enums.CallType SelectedCallType { get; set; } = BO.Enums.CallType.none;

        // Collection of call types for the ComboBox
        public IEnumerable<BO.Enums.CallType> CallTypeCollection { get; private set; }

        // Observable collection for the filtered list of volunteers
        public ObservableCollection<BO.VolunteerInList> FilteredVolunteers { get; private set; }

        // Constructor
        public VolunteerListWindow()
        {
            InitializeComponent();
            CallTypeCollection = Enum.GetValues(typeof(BO.Enums.CallType)).Cast<BO.Enums.CallType>();
            FilteredVolunteers = new ObservableCollection<BO.VolunteerInList>(GetAllVolunteers());
            DataContext = this;
        }

        // Event handler for ComboBox selection change
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterVolunteers();
        }

        // Method to filter the volunteers based on the selected call type
        private void FilterVolunteers()
        {
            if (SelectedCallType == BO.Enums.CallType.none)
            {
                // No filtering, show all volunteers
                FilteredVolunteers = new ObservableCollection<BO.VolunteerInList>(GetAllVolunteers());
            }
            else
            {
                // Filter volunteers by the selected call type
                FilteredVolunteers = new ObservableCollection<BO.VolunteerInList>(
                    GetAllVolunteers().Where(v => v.CallType == SelectedCallType));
            }

            // Notify the UI of the updated collection
            OnPropertyChanged(nameof(FilteredVolunteers));
        }

        // Method to retrieve all volunteers from the BL layer
        private IEnumerable<BO.VolunteerInList> GetAllVolunteers()
        {
            return s_bl.Volunteer.GetVolunteersList();
        }

        // Helper method to notify the UI of property changes
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        // Event for property change notifications
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
