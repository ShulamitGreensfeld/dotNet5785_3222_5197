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

        private readonly Action _refreshListAction;

        // Add CurrentVolunteer property
        public BO.Volunteer? CurrentVolunteer { get; set; }

        // Add VolunteerObserver method
        private void VolunteerObserver()
        {
            if (CurrentVolunteer?.Id != 0)
            {
                int id = CurrentVolunteer.Id;
                CurrentVolunteer = null;
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
            }
        }


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
                if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
                    s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerObserver);
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
                if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
                    s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerObserver);
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
            if (SelectedVolunteer == null || SelectedVolunteer.Id == 0)
            {
                MessageBox.Show("Please select a valid volunteer to view open calls.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var window = new VolunteerWindow(SelectedVolunteer.Id);
            bool? result = window.ShowDialog();
            if (result == true)
            {
                QueryVolunteerList(); // רענון הרשימה אחרי עדכון
            }
        }

        // Opens the "Add New Volunteer" window.
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new VolunteerWindow();
            bool? result = window.ShowDialog();
            if (result == true)
            {
                QueryVolunteerList(); // רענון הרשימה אחרי הוספה
            }
        }

        // Deletes a volunteer after confirming with the user.
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var result = MessageBox.Show("Are you sure you want to delete this volunteer?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Volunteer.DeleteVolunteer(id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Delete failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is BO.VolunteerInList selected)
            {
                SelectedVolunteer = selected; // עדכון המתנדב שנבחר
            }
            else
            {
                SelectedVolunteer = null; // איפוס אם לא נבחר מתנדב
            }
        }



    }
}