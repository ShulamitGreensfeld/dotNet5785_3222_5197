using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlApi;
using PL.Volunteer;

namespace PL
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
    }

    public class LoginViewModel : INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(_ => Login());
        }

        private void Login()
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
                    if (App.IsAdminLoggedIn)
                    {
                        var result = MessageBox.Show(
                            "מנהל כבר מחובר. בחר האם להכנס כמתנדב או לצאת מהמערכת",
                            "כניסת מנהל",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            new VolunteerSelfWindow(parsedId).Show();
                        }
                        // אם לא, לא קורה כלום
                    }
                    else
                    {
                        var result = MessageBox.Show(
                            "Do you want to enter the management screen?",
                            "Login as Manager",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            App.IsAdminLoggedIn = true;
                            new MainWindow().Show();
                        }
                        else
                        {
                            new VolunteerSelfWindow(parsedId).Show();
                        }
                    }
                }
                UserId = "";
                Password = "";
                ErrorMessage = "";
                OnPropertyChanged(nameof(UserId));
                OnPropertyChanged(nameof(Password));
                OnPropertyChanged(nameof(ErrorMessage));
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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public static class PasswordBinding
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBinding),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject obj) =>
            (string)obj.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject obj, string value) =>
            obj.SetValue(BoundPasswordProperty, value);

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordBinding),
                new PropertyMetadata(false, OnBindPasswordChanged));

        public static bool GetBindPassword(DependencyObject obj) =>
            (bool)obj.GetValue(BindPasswordProperty);

        public static void SetBindPassword(DependencyObject obj, bool value) =>
            obj.SetValue(BindPasswordProperty, value);

        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb && (bool)e.NewValue)
                pb.PasswordChanged += (s, ev) => SetBoundPassword(pb, pb.Password);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb && pb.Password != (string)e.NewValue)
            {
                pb.PasswordChanged -= (s, ev) => SetBoundPassword(pb, pb.Password);
                pb.Password = (string)e.NewValue;
                pb.PasswordChanged += (s, ev) => SetBoundPassword(pb, pb.Password);
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object>? _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
