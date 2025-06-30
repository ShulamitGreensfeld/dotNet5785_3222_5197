using BlApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        static readonly IBl s_bl = BlApi.Factory.Get();

        // Event for notifying the UI of property changes
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Currently logged-in volunteer (used for observer)
        private BO.Volunteer? _currentVolunteer;
        public BO.Volunteer? CurrentVolunteer
        {
            get => _currentVolunteer;
            set
            {
                _currentVolunteer = value;
                OnPropertyChanged(nameof(CurrentVolunteer));
            }
        }

        // Filter for the call type dropdown
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
                    QueryVolunteerList();
                }
            }
        }

        // Selected volunteer in the list
        private BO.VolunteerInList? _selectedVolunteer;
        public BO.VolunteerInList? SelectedVolunteer
        {
            get => _selectedVolunteer;
            set
            {
                _selectedVolunteer = value;
                OnPropertyChanged(nameof(SelectedVolunteer));
            }
        }

        // List of volunteers displayed in the view
        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get => (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty);
            set => SetValue(VolunteerListProperty, value);
        }

        // DependencyProperty backing VolunteerList
        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register(
                nameof(VolunteerList),
                typeof(IEnumerable<BO.VolunteerInList>),
                typeof(VolunteerListWindow),
                new PropertyMetadata(null));

        // Constructor initializes the window
        public VolunteerListWindow()
        {
            try
            {
                InitializeComponent();
                // DataContext is set via XAML
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteer window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Runs when the window loads; sets observer and queries list
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
                    s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerObserver);
                QueryVolunteerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteer data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Runs when the window closes; removes observer
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
                    s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error closing volunteer window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Refreshes CurrentVolunteer by fetching it again from BL
        private void VolunteerObserver()
        {
            if (CurrentVolunteer?.Id != 0)
            {
                int id = CurrentVolunteer.Id;
                CurrentVolunteer = null;
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
            }
        }

        // Queries the volunteer list from BL and applies optional filtering
        private void QueryVolunteerList()
        {
            try
            {
                VolunteerList = SelectedCallType == BO.Enums.CallType.none
                    ? s_bl.Volunteer.GetVolunteersList()
                    : s_bl.Volunteer.GetVolunteersFilterList(SelectedCallType);

                if (VolunteerList == null || !VolunteerList.Any())
                {
                    MessageBox.Show("No volunteers found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteer list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Opens the selected volunteer in a window when double-clicked
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVolunteer == null || SelectedVolunteer.Id == 0)
            {
                MessageBox.Show("Please select a valid volunteer to view open calls.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (Window w in Application.Current.Windows)
            {
                if (w is PL.Volunteer.VolunteerWindow volWin && volWin.CurrentVolunteer?.Id == SelectedVolunteer.Id)
                {
                    w.Activate();
                    MessageBox.Show("The volunteer is already open in another window.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            var win = new PL.Volunteer.VolunteerWindow(SelectedVolunteer.Id);
            win.Show();
        }

        // Opens the add volunteer window
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new VolunteerWindow();
            bool? result = window.ShowDialog();
            if (result == true)
            {
                QueryVolunteerList();
            }
        }
    }
}