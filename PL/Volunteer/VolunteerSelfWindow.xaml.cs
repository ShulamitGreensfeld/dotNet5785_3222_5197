using BlApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for VolunteerSelfWindow.xaml
    /// </summary>
    public partial class VolunteerSelfWindow : Window
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        private readonly Action _refreshAction;
        public BO.Volunteer Volunteer { get; set; }
        public IEnumerable<BO.Enums.DistanceTypes> DistanceTypes => Enum.GetValues(typeof(BO.Enums.DistanceTypes)) as BO.Enums.DistanceTypes[];

        public bool HasCallInProgress => Volunteer?.CallInProgress != null;
        public bool CanSelectCall => Volunteer?.CallInProgress == null && Volunteer?.IsActive == true;
        public bool CanSetInactive => Volunteer?.CallInProgress == null;

        public VolunteerSelfWindow(int volunteerId)
        {
            InitializeComponent();
            Volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId);
            _refreshAction = RefreshVolunteer;
            s_bl.Volunteer.AddObserver(Volunteer.Id, _refreshAction);
            //OpenSelectCallForTreatmentCommand = new RelayCommand(_ => OpenSelectCallForTreatmentWindow());
            DataContext = this;
        }

        private void RefreshVolunteer()
        {
            Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
            Dispatcher.Invoke(() =>
            {
                DataContext = null;
                DataContext = this;
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            s_bl.Volunteer.RemoveObserver(Volunteer.Id, _refreshAction);
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PasswordBox passwordBox = this.FindName("PasswordBox") as PasswordBox;
                if (passwordBox != null && !string.IsNullOrWhiteSpace(passwordBox.Password))
                    Volunteer.Password = passwordBox.Password;
                else
                    Volunteer.Password = null;

                s_bl.Volunteer.UpdateVolunteerDetails(Volunteer.Id, Volunteer);
                MessageBox.Show("הפרטים עודכנו בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בעדכון הפרטים: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        //private void btnSelectCall_Click(object sender, RoutedEventArgs e)
        //{
        //    // פתח מסך בחירת קריאה (יש לממש מסך זה)
        //    var selectCallWindow = new SelectCallWindow(Volunteer.Id);
        //    selectCallWindow.ShowDialog();
        //    // רענון נתונים
        //    Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
        //    DataContext = null;
        //    DataContext = this;
        //}

        private void btnFinishCall_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer?.CallInProgress == null) return;
            try
            {
                s_bl.Call.MarkCallCompletion(Volunteer.Id, Volunteer.CallInProgress.Id);
                MessageBox.Show("הטיפול בקריאה הסתיים.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בסיום טיפול: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelCall_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer?.CallInProgress == null) return;
            try
            {
                s_bl.Call.MarkCallCancellation(Volunteer.Id, Volunteer.CallInProgress.Id);
                MessageBox.Show("הטיפול בקריאה בוטל.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בביטול טיפול: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new VolunteerCallHistoryWindow(Volunteer.Id);
            historyWindow.ShowDialog();
        }

        public BO.VolunteerInList? SelectedVolunteer { get; set; }


        private void OpenCallsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer == null || Volunteer.Id == 0)
            {
                MessageBox.Show("לא ניתן לפתוח בחירת קריאה. מתנדב לא קיים או לא נבחר.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Volunteer.MaxDistance.HasValue)
            {
                MessageBox.Show("אנא הגדר מרחק מקסימלי למתנדב.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var selectCallWindow = new SelectCallForTreatmentWindow(
                    Volunteer.Id,
                    Volunteer.FullAddress ?? string.Empty,
                    Volunteer.MaxDistance.Value,
                    Volunteer
                );
                bool? result = selectCallWindow.ShowDialog();
                if (result == true)
                {
                    // רענון מיידי של נתוני המתנדב
                    Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
                    DataContext = null;
                    DataContext = this;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בפתיחת חלון בחירת קריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

