//using System;
//using System.ComponentModel;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using BlApi;
//using PL.Volunteer;
//using PL.Commands;

//namespace PL
//{
//    /// <summary>
//    /// Code-behind for Login Window. Contains only the constructor.
//    /// </summary>
//    public partial class LoginWindow : Window
//    {
//        public LoginWindow()
//        {
//            InitializeComponent();
//        }
//    }

//    /// <summary>
//    /// ViewModel for LoginWindow. Implements login logic, input validation and navigation.
//    /// </summary>
//    public class LoginViewModel : INotifyPropertyChanged
//    {
//        private static readonly IBl s_bl = Factory.Get();

//        public string UserId { get; set; } = "";
//        public string Password { get; set; } = "";

//        private string _errorMessage = "";
//        public string ErrorMessage
//        {
//            get => _errorMessage;
//            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
//        }

//        // Command for login button
//        public ICommand LoginCommand { get; }

//        // Constructor
//        public LoginViewModel()
//        {
//            LoginCommand = new RelayCommand(_ => Login());
//        }

//        /// <summary>
//        /// Handles login process: verifies input, checks credentials via BL, and opens the appropriate window.
//        /// Also ensures that a volunteer cannot log in more than once.
//        /// </summary>
//        /// <summary>
//        /// Handles login process: validates input, checks credentials via BL, and opens appropriate window.
//        /// Prevents multiple admin logins by using a BL-based internal flag.
//        /// </summary>
//        private void Login()
//        {
//            ErrorMessage = "";

//            try
//            {
//                var role = s_bl.Volunteer.EnterSystem(UserId, Password);

//                int volunteerId = int.Parse(UserId);

//                if (role == BO.Enums.Role.volunteer)
//                {
//                    new VolunteerSelfWindow(volunteerId).Show();
//                }
//                else if (role == BO.Enums.Role.manager)
//                {
//                    var result = MessageBox.Show(
//                        "Do you want to enter the management screen?",
//                        "Login as Manager",
//                        MessageBoxButton.YesNo,
//                        MessageBoxImage.Question);

//                    if (result == MessageBoxResult.Yes)
//                    {
//                        App.IsAdminLoggedIn = true;
//                        App.LoggedAdminId = adminId;
//                        new MainWindow().Show();
//                    }
//                    else
//                    {
//                        new VolunteerSelfWindow(volunteerId).Show();
//                    }
//                }

//                // Clear sensitive data
//                UserId = "";
//                Password = "";
//                ErrorMessage = "";
//                OnPropertyChanged(nameof(UserId));
//                OnPropertyChanged(nameof(Password));
//                OnPropertyChanged(nameof(ErrorMessage));
//            }
//            catch (BO.BlInvalidFormatException ex)
//            {
//                ErrorMessage = ex.Message;
//            }
//            catch (BO.BlUnauthorizedException ex)
//            {
//                ErrorMessage = ex.Message;
//            }
//            catch (BO.BlDoesNotExistException)
//            {
//                ErrorMessage = "User not found.";
//            }
//            catch (Exception ex)
//            {
//                ErrorMessage = $"Unexpected error: {ex.Message}";
//            }
//        }

//        public event PropertyChangedEventHandler? PropertyChanged;
//        private void OnPropertyChanged(string name) =>
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }

//    /// <summary>
//    /// Helper class for binding PasswordBox via attached property
//    /// </summary>
//    public static class PasswordBinding
//    {
//        public static readonly DependencyProperty BoundPasswordProperty =
//            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBinding),
//                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

//        public static string GetBoundPassword(DependencyObject obj) =>
//            (string)obj.GetValue(BoundPasswordProperty);

//        public static void SetBoundPassword(DependencyObject obj, string value) =>
//            obj.SetValue(BoundPasswordProperty, value);

//        public static readonly DependencyProperty BindPasswordProperty =
//            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordBinding),
//                new PropertyMetadata(false, OnBindPasswordChanged));

//        public static bool GetBindPassword(DependencyObject obj) =>
//            (bool)obj.GetValue(BindPasswordProperty);

//        public static void SetBindPassword(DependencyObject obj, bool value) =>
//            obj.SetValue(BindPasswordProperty, value);

//        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            if (d is PasswordBox pb && (bool)e.NewValue)
//                pb.PasswordChanged += (s, ev) => SetBoundPassword(pb, pb.Password);
//        }

//        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            if (d is PasswordBox pb && pb.Password != (string)e.NewValue)
//            {
//                pb.PasswordChanged -= (s, ev) => SetBoundPassword(pb, pb.Password);
//                pb.Password = (string)e.NewValue;
//                pb.PasswordChanged += (s, ev) => SetBoundPassword(pb, pb.Password);
//            }
//        }
//    }
//}
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlApi;
using PL.Volunteer;
using PL.Commands;

namespace PL
{
    /// <summary>
    /// Code-behind for Login Window. Contains only the constructor.
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// ViewModel for LoginWindow. Implements login logic, input validation and navigation.
    /// </summary>
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

        /// <summary>
        /// Handles login process: validates input, checks credentials via BL, and opens appropriate window.
        /// Prevents multiple logins by checking BL restrictions.
        /// </summary>
        private void Login()
        {
            ErrorMessage = "";

            try
            {
                var role = s_bl.Volunteer.EnterSystem(UserId, Password);
                int volunteerId;
                if (!int.TryParse(UserId, out volunteerId))
                {
                    ErrorMessage = "Invalid ID format.";
                    return;
                }

                var volunteer = s_bl.Volunteer.GetVolunteersList().FirstOrDefault(v => v.Id == volunteerId);
                if (volunteer == null)
                {
                    ErrorMessage = "Volunteer not found.";
                    return;
                }

                //int volunteerId = int.Parse(UserId);

                if (role == BO.Enums.Role.volunteer)
                {
                    new VolunteerSelfWindow(volunteerId).Show();
                }
                else if (role == BO.Enums.Role.manager)
                {
                    var result = MessageBox.Show(
                        "Do you want to enter the management screen?",
                        "Login as Manager",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        App.IsAdminLoggedIn = true;
                        App.LoggedAdminId = volunteerId; // ✅ this is the fix
                        new MainWindow().Show();
                    }
                    else
                    {
                        new VolunteerSelfWindow(volunteerId).Show();
                    }
                }

                // Clear sensitive data
                UserId = "";
                Password = "";
                ErrorMessage = "";
                OnPropertyChanged(nameof(UserId));
                OnPropertyChanged(nameof(Password));
                OnPropertyChanged(nameof(ErrorMessage));
            }
            catch (BO.BlInvalidFormatException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (BO.BlUnauthorizedException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (BO.BlDoesNotExistException)
            {
                ErrorMessage = "User not found.";
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

    /// <summary>
    /// Helper class for binding PasswordBox via attached property
    /// </summary>
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
}