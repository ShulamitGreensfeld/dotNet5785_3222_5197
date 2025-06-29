
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
        private static readonly HashSet<int> _openVolunteerWindows = new();


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
                MessageBox.Show("יש לבחור מתנדב תקין.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int id = SelectedVolunteer.Id;

            if (_openVolunteerWindows.Contains(id))
            {
                MessageBox.Show("החלון של מתנדב זה כבר פתוח.", "שים לב", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _openVolunteerWindows.Add(id);

            var window = new VolunteerWindow(id);
            window.Closed += (_, _) => _openVolunteerWindows.Remove(id);

            bool? result = window.ShowDialog(); // השתמש במקום Show()
            if (result == true)
                QueryVolunteerList();
        }



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