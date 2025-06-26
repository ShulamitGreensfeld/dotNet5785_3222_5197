using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BlApi;
using PL.Volunteer;

namespace PL
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordBox.Password;
            _viewModel.Login(() => this.Close());
        }

        private class LoginViewModel : INotifyPropertyChanged
        {
            private static readonly IBl s_bl = Factory.Get();

            private string _userId = "";
            public string UserId
            {
                get => _userId;
                set
                {
                    _userId = value;
                    OnPropertyChanged(nameof(UserId));
                }
            }

            public string Password { get; set; } = "";

            private string _errorMessage = "";
            public string ErrorMessage
            {
                get => _errorMessage;
                set
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public void Login(Action closeWindow)
            {
                ErrorMessage = "";

                if (string.IsNullOrWhiteSpace(UserId))
                {
                    ErrorMessage = "User ID is required.";
                    return;
                }

                if (!int.TryParse(UserId, out int parsedId))
                {
                    ErrorMessage = "Invalid ID format.";
                    return;
                }

                try
                {
                    App.CurrentUserId = parsedId;
                    var role = s_bl.Volunteer.EnterSystem(UserId, Password);

                    if (role == BO.Enums.Role.volunteer)
                    {
                        new VolunteerSelfWindow(parsedId).Show();
                    }
                    else if (role == BO.Enums.Role.manager)
                    {
                        var result = MessageBox.Show("Do you want to enter the management screen?", "Login as Manager",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                            new MainWindow().Show();
                        else
                            new VolunteerSelfWindow(parsedId).Show();
                    }

                    closeWindow();
                }
                catch (BO.BlDoesNotExistException)
                {
                    ErrorMessage = "User not found.";
                }
                catch (BO.BlInvalidFormatException)
                {
                    ErrorMessage = "Invalid ID or password.";
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Unexpected error: {ex.Message}";
                }
            }
        }
    }
}