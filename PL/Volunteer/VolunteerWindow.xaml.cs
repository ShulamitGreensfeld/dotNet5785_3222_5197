//using System.Windows;
//using Helpers;
//using BlApi;
//using System.Windows.Controls;
//using System.ComponentModel;
//using System.Windows.Input;

//namespace PL.Volunteer
//{
//    public partial class VolunteerWindow : Window, INotifyPropertyChanged
//    {
//        // BL service instance
//        private static readonly IBl s_bl = BlApi.Factory.Get();

//        public event PropertyChangedEventHandler? PropertyChanged;

//        /// <summary>
//        /// Raise the PropertyChanged event for the given property name.
//        /// </summary>
//        private void OnPropertyChanged(string propertyName)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }

//        // Dependency property holding the volunteer being edited or created
//        public BO.Volunteer? CurrentVolunteer
//        {
//            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
//            set
//            {
//                SetValue(CurrentVolunteerProperty, value);
//                OnPropertyChanged(nameof(CanChooseCall));
//                OnPropertyChanged(nameof(CanSetInactive));
//            }
//        }

//        public static readonly DependencyProperty CurrentVolunteerProperty =
//            DependencyProperty.Register(nameof(CurrentVolunteer), typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

//        // Dependency property holding the text displayed on the action button ("Add" or "Update")
//        public string ButtonText
//        {
//            get { return (string)GetValue(ButtonTextProperty); }
//            set { SetValue(ButtonTextProperty, value); }
//        }

//        public static readonly DependencyProperty ButtonTextProperty =
//            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(VolunteerWindow), new PropertyMetadata("Add"));

//        /// <summary>
//        /// Constructor for creating or updating a volunteer.
//        /// </summary>
//        public VolunteerWindow(int id = 0)
//        {
//            InitializeComponent();

//            if (id != 0)
//            {
//                try
//                {
//                    CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
//                    CurrentVolunteer.Password = string.Empty;
//                    ButtonText = "Update";
//                }
//                catch
//                {
//                    CurrentVolunteer = new BO.Volunteer();
//                    ButtonText = "Add";
//                }
//            }
//            else
//            {
//                CurrentVolunteer = new BO.Volunteer();
//                ButtonText = "Add";
//            }

//            DataContext = this;
//        }

//        /// <summary>
//        /// Handles add or update action when the main button is clicked.
//        /// </summary>
//        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                if (ButtonText == "Add")
//                {
//                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
//                    CurrentVolunteer.Password = string.Empty;
//                    MessageBox.Show("Volunteer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
//                    Close();
//                }
//                else // Update
//                {
//                    if (string.IsNullOrWhiteSpace(CurrentVolunteer.Password))
//                        CurrentVolunteer.Password = null;
//                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer!.Id, CurrentVolunteer);
//                    CurrentVolunteer.Password = string.Empty;
//                    MessageBox.Show("Volunteer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
//                    Close();
//                }
//            }
//            catch (System.Exception ex)
//            {
//                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                Console.WriteLine($"Error details: {ex}");
//            }
//        }

//        /// <summary>
//        /// Deletes the volunteer if possible.
//        /// </summary>
//        private void btnDelete_Click(object sender, RoutedEventArgs e)
//        {
//            if (CurrentVolunteer == null || CurrentVolunteer.Id == 0)
//                return;

//            try
//            {
//                if (MessageBox.Show("Are you sure you want to delete this volunteer?", "Confirm Delete",
//                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
//                {
//                    s_bl.Volunteer.DeleteVolunteer(CurrentVolunteer.Id);
//                    MessageBox.Show("Volunteer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
//                    Close();
//                }
//            }
//            catch (BO.BlDeletionException)
//            {
//                MessageBox.Show("You cannot delete a volunteer who has calls (even historical ones).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//            catch (System.Exception ex)
//            {
//                MessageBox.Show($"Unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        /// <summary>
//        /// Refreshes the volunteer object by re-fetching it from BL.
//        /// </summary>
//        private void VolunteerObserver()
//        {
//            if (CurrentVolunteer?.Id != 0)
//            {
//                int id = CurrentVolunteer.Id;
//                CurrentVolunteer = null;
//                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
//            }
//        }

//        /// <summary>
//        /// Registers volunteer observer on window load.
//        /// </summary>
//        private void Window_Loaded(object sender, RoutedEventArgs e)
//        {
//            if (CurrentVolunteer?.Id != 0)
//                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerObserver);
//        }

//        /// <summary>
//        /// Unregisters volunteer observer on window close.
//        /// </summary>
//        private void Window_Closed(object sender, System.EventArgs e)
//        {
//            if (CurrentVolunteer?.Id != 0)
//                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerObserver);
//        }

//        /// <summary>
//        /// Opens a read-only window showing current call details if exists.
//        /// </summary>
//        private void ViewCurrentCallDetails_Click(object sender, RoutedEventArgs e)
//        {
//            if (CurrentVolunteer == null || CurrentVolunteer.CallInProgress == null)
//            {
//                MessageBox.Show("This volunteer has no call in progress.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
//                return;
//            }

//            try
//            {
//                var currentCall = s_bl.Call.GetCallDetails(CurrentVolunteer.CallInProgress.CallId);
//                var callDetailsWindow = new PL.Call.CallDetailsWindow(currentCall.Id, CurrentVolunteer.Id);
//                callDetailsWindow.Show();
//            }
//            catch (System.Exception ex)
//            {
//                MessageBox.Show($"Error retrieving call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        /// <summary>
//        /// Indicates whether the volunteer can choose a call.
//        /// </summary>
//        public bool CanChooseCall => CurrentVolunteer?.CallInProgress == null;

//        /// <summary>
//        /// Indicates whether the volunteer can be marked as inactive.
//        /// </summary>
//        public bool CanSetInactive => CurrentVolunteer?.CallInProgress == null;

//        /// <summary>
//        /// Default constructor (used for adding new volunteers).
//        /// </summary>
//        public VolunteerWindow() : this(0) { }
//    }
//}
using System.Windows;
using Helpers;
using BlApi;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace PL.Volunteer
{
    public partial class VolunteerWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set
            {
                SetValue(CurrentVolunteerProperty, value);
                OnPropertyChanged(nameof(CanChooseCall));
                OnPropertyChanged(nameof(CanSetInactive));
            }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register(nameof(CurrentVolunteer), typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(VolunteerWindow), new PropertyMetadata("Add"));

        private volatile DispatcherOperation? _observerOperation = null;

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
                    CurrentVolunteer = new BO.Volunteer();
                    ButtonText = "Add";
                }
            }
            else
            {
                CurrentVolunteer = new BO.Volunteer();
                ButtonText = "Add";
            }

            DataContext = this;
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add")
                {
                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    CurrentVolunteer.Password = string.Empty;
                    MessageBox.Show("Volunteer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else // Update
                {
                    if (string.IsNullOrWhiteSpace(CurrentVolunteer.Password))
                        CurrentVolunteer.Password = null;
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer!.Id, CurrentVolunteer);
                    CurrentVolunteer.Password = string.Empty;
                    MessageBox.Show("Volunteer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error details: {ex}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer == null || CurrentVolunteer.Id == 0)
                return;

            try
            {
                if (MessageBox.Show("Are you sure you want to delete this volunteer?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    s_bl.Volunteer.DeleteVolunteer(CurrentVolunteer.Id);
                    MessageBox.Show("Volunteer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
            catch (BO.BlDeletionException)
            {
                MessageBox.Show("You cannot delete a volunteer who has calls (even historical ones).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolunteerObserver()
        {
            if (_observerOperation is null || _observerOperation.Status == DispatcherOperationStatus.Completed)
            {
                _observerOperation = Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (CurrentVolunteer?.Id != 0)
                    {
                        int id = CurrentVolunteer.Id;
                        CurrentVolunteer = null;
                        CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
                    }
                }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.Id != 0)
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerObserver);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (CurrentVolunteer?.Id != 0)
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerObserver);
        }

        private void ViewCurrentCallDetails_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer == null || CurrentVolunteer.CallInProgress == null)
            {
                MessageBox.Show("This volunteer has no call in progress.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var currentCall = s_bl.Call.GetCallDetails(CurrentVolunteer.CallInProgress.CallId);
                var callDetailsWindow = new PL.Call.CallDetailsWindow(currentCall.Id, CurrentVolunteer.Id);
                callDetailsWindow.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error retrieving call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CanChooseCall => CurrentVolunteer?.CallInProgress == null;
        public bool CanSetInactive => CurrentVolunteer?.CallInProgress == null;

        public VolunteerWindow() : this(0) { }
    }
}
