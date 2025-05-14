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
                MessageBox.Show("שגיאה בטעינת רשימת מתנדבים: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolunteerListObserver() => QueryVolunteerList();

        public VolunteerListWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת חלון מתנדבים: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            QueryVolunteerList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                s_bl.Volunteer.AddObserver(VolunteerListObserver);
                QueryVolunteerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת נתוני מתנדבים: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            try
            {
                s_bl.Volunteer.RemoveObserver(VolunteerListObserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בסגירת חלון מתנדבים: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public BO.VolunteerInList? SelectedVolunteer { get; set; }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVolunteer != null)
            {
                var window = new VolunteerWindow(SelectedVolunteer.Id);
                window.Show();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new VolunteerWindow();
            window.Show();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var result = MessageBox.Show("האם אתה בטוח שברצונך למחוק את המתנדב?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Volunteer.DeleteVolunteer(id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("המחיקה נכשלה: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
