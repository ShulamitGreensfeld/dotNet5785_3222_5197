using System.Windows;
using Helpers;
using BlApi;
using System.Windows.Controls;

namespace PL.Volunteer
{
    public partial class VolunteerWindow : Window
    {
        // BL access object
        private static readonly IBl s_bl = BlApi.Factory.Get();

        // Dependency property for the Volunteer object
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
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
        }

        /// <summary>
        /// Handles the Add/Update button click. Adds a new volunteer or updates an existing one.
        /// </summary>
        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PasswordBox passwordBox = this.FindName("PasswordBox") as PasswordBox;

                if (ButtonText == "Add")
                {
                    if (passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password))
                        CurrentVolunteer!.Password = null;
                    else
                        CurrentVolunteer!.Password = passwordBox?.Password;

                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    MessageBox.Show("Volunteer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Update
                {
                    var oldVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer!.Id);

                    if (passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password))
                        CurrentVolunteer.Password = oldVolunteer.Password; // Retain original password
                    else
                        CurrentVolunteer.Password = passwordBox?.Password;

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
    }
}