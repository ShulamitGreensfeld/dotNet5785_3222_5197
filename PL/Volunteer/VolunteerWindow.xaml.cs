using System.Windows;
using Helpers;
using BlApi;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace PL.Volunteer
{
    public partial class VolunteerWindow : Window, INotifyPropertyChanged
    {
        // BL access object
        private static readonly IBl s_bl = BlApi.Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Dependency property for the Volunteer object
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set
            {
                SetValue(CurrentVolunteerProperty, value);
                OnPropertyChanged(nameof(CanChooseCall)); // Notify when CurrentVolunteer changes
            }
        }
        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register(nameof(CurrentVolunteer), typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

        // Dependency property for the button text
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(VolunteerWindow), new PropertyMetadata("Add"));

        /// <summary>
        /// General constructor for the window, optionally loads an existing volunteer by ID.
        /// </summary>
        public VolunteerWindow(int id = 0)
        {
            InitializeComponent();
            if (id != 0)
            {
                try
                {
                    CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
                    CurrentVolunteer.Password = string.Empty;
                    ButtonText = "Update";
                }
                catch
                {
                    // Handle error: fallback to new volunteer
                    CurrentVolunteer = new BO.Volunteer();
                    ButtonText = "Add";
                }
            }
            else
            {
                CurrentVolunteer = new BO.Volunteer();
                ButtonText = "Add";
            }

            // Ensure DataContext is set for binding
            DataContext = this;
        }

        /// <summary>
        /// Handles the Add/Update button click. Adds a new volunteer or updates an existing one.
        /// </summary>
        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PasswordBox? passwordBox = this.FindName("PasswordBox") as PasswordBox;

                if (ButtonText == "Add")
                {
                    CurrentVolunteer!.Password = passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password)
                        ? null
                        : passwordBox?.Password;

                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    MessageBox.Show("Volunteer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Update
                {
                    var oldVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer!.Id);

                    CurrentVolunteer.Password = passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password)
                        ? oldVolunteer.Password
                        : passwordBox?.Password;

                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);
                    MessageBox.Show("Volunteer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Close();
            }
            catch (System.Exception ex)
            {
                string userFriendlyMessage;

                if (ex is ArgumentNullException)
                    userFriendlyMessage = "אחד השדות החיוניים חסר. אנא מלא את כל השדות הנדרשים.";
                else if (ex is InvalidOperationException)
                    userFriendlyMessage = "הפעולה אינה חוקית במצב הנוכחי.";
                else if (ex is BO.BlDoesNotExistException)
                    userFriendlyMessage = "המתנדב לא נמצא במערכת.";
                else
                    userFriendlyMessage = "אירעה שגיאה בלתי צפויה. אנא נסה שוב.";

                MessageBox.Show(userFriendlyMessage, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error details: {ex}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer == null || CurrentVolunteer.Id == 0)
                return;
            try
            {
                if (MessageBox.Show("Are you sure you want to delete this volunteer?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    s_bl.Volunteer.DeleteVolunteer(CurrentVolunteer.Id);
                    MessageBox.Show("Volunteer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
            catch (System.Exception ex)
            {
                string userFriendlyMessage = ex is BO.BlDoesNotExistException
                    ? "המתנדב לא נמצא במערכת."
                    : "אירעה שגיאה בלתי צפויה. אנא נסה שוב.";
                MessageBox.Show(userFriendlyMessage, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error details: {ex}");
            }
        }

        /// <summary>
        /// Reloads the current volunteer's data from BL if an ID is set.
        /// </summary>
        private void VolunteerObserver()
        {
            if (CurrentVolunteer?.Id != 0)
            {
                int id = CurrentVolunteer.Id;
                CurrentVolunteer = null;
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
            }
        }

        /// <summary>
        /// Registers the volunteer observer when the window is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.Id != 0)
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerObserver);
        }

        /// <summary>
        /// Unregisters the volunteer observer when the window is closed.
        /// </summary>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (CurrentVolunteer?.Id != 0)
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerObserver);
        }



        private void OpenVolunteerCallsHistoryWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentVolunteer == null || CurrentVolunteer.Id == 0)
                {
                    MessageBox.Show("לא ניתן לפתוח היסטוריית קריאות. מתנדב לא קיים או לא נבחר.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var callHistoryWindow = new VolunteerCallHistoryWindow(CurrentVolunteer.Id);
                callHistoryWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public BO.VolunteerInList? SelectedVolunteer { get; set; }



        private void ViewCurrentCallDetails_Click(object sender, RoutedEventArgs e)
        {
            // בדיקה אם יש למתנדב קריאה בטיפול
            if (CurrentVolunteer == null || CurrentVolunteer.CallInProgress == null)
            {
                MessageBox.Show("למתנדב זה אין כרגע קריאה בטיפול.", "מידע", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // שליפת פרטי הקריאה המלאים
                var currentCall = s_bl.Call.GetCallDetails(CurrentVolunteer.CallInProgress.CallId);

                // פתיחת חלון צפייה בלבד עם כל הפרטים כולל מרחק
                var singleCallWindow = new PL.Call.SingleCallWindow(
                    currentCall,
                    CurrentVolunteer.CallInProgress.CallDistance,
                    isReadOnly: true
                );
                singleCallWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בשליפת פרטי הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CanChooseCall => CurrentVolunteer?.CallInProgress == null;


        public VolunteerWindow()
        {
            InitializeComponent();

            DataContext = this;
        }
    }
}
