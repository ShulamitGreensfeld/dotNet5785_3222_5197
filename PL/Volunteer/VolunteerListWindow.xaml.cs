//using BlApi;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;

//namespace PL.Volunteer
//{
//    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
//    {
//        static readonly IBl s_bl = BlApi.Factory.Get();

//        private readonly Action _refreshListAction;

//        // Add CurrentVolunteer property
//        public BO.Volunteer? CurrentVolunteer { get; set; }

//        // Add VolunteerObserver method
//        private void VolunteerObserver()
//        {
//            if (CurrentVolunteer?.Id != 0)
//            {
//                int id = CurrentVolunteer.Id;
//                CurrentVolunteer = null;
//                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
//            }
//        }


//        public IEnumerable<BO.VolunteerInList> VolunteerList
//        {
//            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
//            set { SetValue(VolunteerListProperty, value); }
//        }

//        public static readonly DependencyProperty VolunteerListProperty =
//            DependencyProperty.Register(
//                nameof(VolunteerList),
//                typeof(IEnumerable<BO.VolunteerInList>),
//                typeof(VolunteerListWindow),
//                new PropertyMetadata(null));

//        public BO.Enums.CallType SelectedCallType { get; set; } = BO.Enums.CallType.none;

//        // Queries the list of volunteers based on the selected call type filter.
//        private void QueryVolunteerList()
//        {
//            try
//            {
//                if (SelectedCallType == BO.Enums.CallType.none)
//                    VolunteerList = s_bl.Volunteer.GetVolunteersList();
//                else
//                    VolunteerList = s_bl.Volunteer.GetVolunteersFilterList(SelectedCallType);

//                if (VolunteerList == null || !VolunteerList.Any())
//                {
//                    MessageBox.Show("No volunteers found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error loading volunteer list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        // Constructor - Initializes the volunteer list window.
//        public VolunteerListWindow()
//        {
//            try
//            {
//                InitializeComponent();
//                DataContext = new VolunteerListWindow();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error loading volunteer window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        // Handles change of selection in the filter combo box.
//        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            QueryVolunteerList();
//        }

//        // Loads initial data and subscribes to volunteer list updates when the window loads.
//        private void Window_Loaded(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
//                    s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerObserver);
//                QueryVolunteerList();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error loading volunteer data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        // Unsubscribes from volunteer list updates when the window is closed.
//        private void Window_Closed(object sender, EventArgs e)
//        {
//            try
//            {
//                if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
//                    s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerObserver);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error closing volunteer window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        public BO.VolunteerInList? SelectedVolunteer { get; set; }

//        // Opens the selected volunteer in a new window when double-clicked.
//        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
//        {
//            if (SelectedVolunteer == null || SelectedVolunteer.Id == 0)
//            {
//                MessageBox.Show("Please select a valid volunteer to view open calls.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                return;
//            }
//            var window = new VolunteerWindow(SelectedVolunteer.Id);
//            bool? result = window.ShowDialog();
//            if (result == true)
//            {
//                QueryVolunteerList(); // רענון הרשימה אחרי עדכון
//            }
//        }

//        // Opens the "Add New Volunteer" window.
//        private void AddButton_Click(object sender, RoutedEventArgs e)
//        {
//            var window = new VolunteerWindow();
//            bool? result = window.ShowDialog();
//            if (result == true)
//            {
//                QueryVolunteerList(); // רענון הרשימה אחרי הוספה
//            }
//        }
//        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            if (sender is ListView listView && listView.SelectedItem is BO.VolunteerInList selected)
//            {
//                SelectedVolunteer = selected;
//            }
//            else
//            {
//                SelectedVolunteer = null;
//            }
//        }
//    }
//}
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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
                    QueryVolunteerList(); // קריאה אחת ל-BL מותרת
                }
            }
        }

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

        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get => (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty);
            set => SetValue(VolunteerListProperty, value);
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register(
                nameof(VolunteerList),
                typeof(IEnumerable<BO.VolunteerInList>),
                typeof(VolunteerListWindow),
                new PropertyMetadata(null));

        public VolunteerListWindow()
        {
            try
            {
                InitializeComponent();
                // אין להגדיר DataContext כאן
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading volunteer window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        private void VolunteerObserver()
        {
            if (CurrentVolunteer?.Id != 0)
            {
                int id = CurrentVolunteer.Id;
                CurrentVolunteer = null;
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
            }
        }

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
                QueryVolunteerList(); // רענון הרשימה לאחר עדכון
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new VolunteerWindow();
            bool? result = window.ShowDialog();
            if (result == true)
            {
                QueryVolunteerList(); // רענון הרשימה לאחר הוספה
            }
        }
    }
}