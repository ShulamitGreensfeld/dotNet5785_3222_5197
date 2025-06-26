using BlApi;
using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;

namespace PL.Volunteer
{
    public partial class VolunteerSelfWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        private readonly Action _refreshAction;

        private BO.Volunteer _volunteer;
        public BO.Volunteer Volunteer
        {
            get => _volunteer;
            set
            {
                _volunteer = value;
                OnPropertyChanged(nameof(Volunteer));
                OnPropertyChanged(nameof(HasCallInProgress));
                OnPropertyChanged(nameof(CanSelectCall));
                OnPropertyChanged(nameof(CanSetInactive));
            }
        }

        public IEnumerable<BO.Enums.DistanceTypes> DistanceTypes =>
            Enum.GetValues(typeof(BO.Enums.DistanceTypes)) as BO.Enums.DistanceTypes[];

        public bool HasCallInProgress => Volunteer?.CallInProgress != null;
        public bool CanSelectCall => Volunteer?.CallInProgress == null && Volunteer?.IsActive == true;
        public bool CanSetInactive => Volunteer?.CallInProgress == null;

        public VolunteerSelfWindow(int volunteerId)
        {
            InitializeComponent();
            Volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId);
            Volunteer.Password = string.Empty;
            _refreshAction = RefreshVolunteer;
            s_bl.Volunteer.AddObserver(Volunteer.Id, _refreshAction);
            DataContext = this;
        }

        private void RefreshVolunteer()
        {
            Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
            Volunteer.Password = string.Empty;
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
                if (string.IsNullOrWhiteSpace(Volunteer.Password))
                    Volunteer.Password = null;
                s_bl.Volunteer.UpdateVolunteerDetails(Volunteer.Id, Volunteer);
                MessageBox.Show("הפרטים עודכנו בהצלחה!", "עדכון פרטי מתנדב", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch
            {
                MessageBox.Show("אירעה שגיאה במהלך עדכון פרטי המתנדב. ודא שכל הפרטים תקינים ונסה שוב.",
                                "שגיאת עדכון", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnFinishCall_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer?.CallInProgress == null) return;

            try
            {
                s_bl.Call.MarkCallCompletion(Volunteer.Id, Volunteer.CallInProgress.Id);
                MessageBox.Show("הטיפול בקריאה סומן כהושלם בהצלחה.", "סיום טיפול", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshVolunteer();
            }
            catch
            {
                MessageBox.Show("לא ניתן היה לסיים את הטיפול בקריאה. נסה שוב מאוחר יותר.",
                                "שגיאת סיום קריאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelCall_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer?.CallInProgress == null) return;

            try
            {
                s_bl.Call.MarkCallCancellation(Volunteer.Id, Volunteer.CallInProgress.Id);
                MessageBox.Show("הקריאה סומנה כמבוטלת.", "ביטול קריאה", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshVolunteer();
            }
            catch
            {
                MessageBox.Show("לא ניתן היה לבטל את הקריאה. נסה שוב מאוחר יותר.",
                                "שגיאת ביטול קריאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new VolunteerCallHistoryWindow(Volunteer.Id);
            historyWindow.ShowDialog();
        }

        private void OpenCallsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer == null || Volunteer.Id == 0)
            {
                MessageBox.Show("לא ניתן לפתוח את מסך הקריאות. פרטי מתנדב חסרים.",
                                "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Volunteer.MaxDistance.HasValue)
            {
                MessageBox.Show("יש להגדיר תחילה מרחק מקסימלי למתנדב כדי לבחור קריאה.",
                                "שגיאת מרחק", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    RefreshVolunteer();
            }
            catch
            {
                MessageBox.Show("לא ניתן היה לפתוח את חלון הקריאות כעת. נסה שנית מאוחר יותר.",
                                "שגיאת פתיחה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}