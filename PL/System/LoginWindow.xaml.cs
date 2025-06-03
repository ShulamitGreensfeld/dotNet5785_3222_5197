using System.Windows;
using BlApi;
using PL.Volunteer;

namespace PL
{
    public partial class LoginWindow : Window
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userId = UserIdTextBox.Text;
            string password = PasswordBox.Password;

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(userId))
                {
                    ErrorMessageTextBlock.Text = "User ID is required.";
                    return;
                }
                App.CurrentUserId = int.Parse(userId); 

                // Authenticate user
                var role = s_bl.Volunteer.EnterSystem(userId, password);

                // Navigate to the appropriate screen
                if (role == BO.Enums.Role.volunteer)
                {
                    new VolunteerWindow(int.Parse(userId)).Show();
                }
                else if (role == BO.Enums.Role.manager)
                {
                    var result = MessageBox.Show("Do you want to enter the management screen?", "Login as Manager",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        new MainWindow().Show();
                    }
                    else
                    {
                        new VolunteerWindow(int.Parse(userId)).Show();
                    }
                }
                Close();
            }
            catch (BO.BlDoesNotExistException)
            {
                ErrorMessageTextBlock.Text = "User not found.";
            }
            catch (BO.BlInvalidFormatException)
            {
                ErrorMessageTextBlock.Text = "Invalid ID or password.";
            }
            catch (System.Exception ex)
            {
                ErrorMessageTextBlock.Text = $"Unexpected error: {ex.Message}";
            }
        }
    }
}
