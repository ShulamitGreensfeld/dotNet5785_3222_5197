using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlApi;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window
    {
        static readonly IBl s_bl = BlApi.Factory.Get();

        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
            set { SetValue(VolunteerListProperty, value); }
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register(
                nameof(VolunteerList),
                typeof(IEnumerable<BO.VolunteerInList>),
                typeof(VolunteerListWindow),
                new PropertyMetadata(null));

        public BO.Enums.CallType SelectedCallType { get; set; } = BO.Enums.CallType.none;

        // Queries the list of volunteers based on the selected call type filter.
        private void QueryVolunteerList()
        {
            try
            {
                if (SelectedCallType == BO.Enums.CallType.none)
                    VolunteerList = s_bl.Volunteer.GetVolunteersList();
                else
                    VolunteerList = s_bl.Volunteer.GetVolunteersFilterList(SelectedCallType);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteer list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Observer method that triggers the list query.
        private void VolunteerListObserver() => QueryVolunteerList();

        // Constructor - Initializes the volunteer list window.
        public VolunteerListWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteer window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handles change of selection in the filter combo box.
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QueryVolunteerList();
        }

        // Loads initial data and subscribes to volunteer list updates when the window loads.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Volunteer.AddObserver(VolunteerListObserver);
                QueryVolunteerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteer data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Unsubscribes from volunteer list updates when the window is closed.
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                s_bl.Volunteer.RemoveObserver(VolunteerListObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error closing volunteer window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public BO.VolunteerInList? SelectedVolunteer { get; set; }

        // Opens the selected volunteer in a new window when double-clicked.
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVolunteer != null)
            {
                var window = new VolunteerWindow(SelectedVolunteer.Id);
                window.Show();
            }
        }

        // Opens the "Add New Volunteer" window.
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new VolunteerWindow();
            window.Show();
        }

        // Currently not used – can be used to handle selection change in the volunteer list.
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}