//using BlApi;
//using System;
//using System.Collections.Generic;
//using System.Windows;
//using System.ComponentModel;

//namespace PL.Volunteer
//{
//    public partial class VolunteerSelfWindow : Window, INotifyPropertyChanged
//    {
//        private static readonly IBl s_bl = BlApi.Factory.Get();

//        private readonly Action _refreshAction;
//        private static readonly HashSet<int> _openHistoryWindows = new();

//        /// <summary>
//        /// The volunteer object bound to the UI.
//        /// </summary>
//        private BO.Volunteer _volunteer;
//        public BO.Volunteer Volunteer
//        {
//            get => _volunteer;
//            set
//            {
//                _volunteer = value;
//                OnPropertyChanged(nameof(Volunteer));
//                OnPropertyChanged(nameof(HasCallInProgress));
//                OnPropertyChanged(nameof(CanSelectCall));
//                OnPropertyChanged(nameof(CanSetInactive));
//            }
//        }

//        /// <summary>
//        /// Available distance types for ComboBox binding.
//        /// </summary>
//        public IEnumerable<BO.Enums.DistanceTypes> DistanceTypes =>
//            Enum.GetValues(typeof(BO.Enums.DistanceTypes)) as BO.Enums.DistanceTypes[];

//        /// <summary>
//        /// Indicates whether the volunteer has a call in progress.
//        /// </summary>
//        public bool HasCallInProgress => Volunteer?.CallInProgress != null;

//        /// <summary>
//        /// Determines whether the volunteer can currently select a new call.
//        /// </summary>
//        public bool CanSelectCall => Volunteer?.CallInProgress == null && Volunteer?.IsActive == true;

//        /// <summary>
//        /// Determines whether the "IsActive" field can be edited.
//        /// </summary>
//        public bool CanSetInactive => Volunteer?.CallInProgress == null;

//        /// <summary>
//        /// Constructor that loads volunteer details and sets observer.
//        /// </summary>
//        public VolunteerSelfWindow(int volunteerId)
//        {
//            InitializeComponent();
//            Volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId);
//            Volunteer.Password = string.Empty;
//            _refreshAction = RefreshVolunteer;
//            s_bl.Volunteer.AddObserver(Volunteer.Id, _refreshAction);
//            DataContext = this;
//        }

//        /// <summary>
//        /// Refreshes the volunteer's data from the BL.
//        /// </summary>
//        private void RefreshVolunteer()
//        {
//            Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
//            Volunteer.Password = string.Empty;
//        }

//        /// <summary>
//        /// Called when window is closed; removes the observer from BL.
//        /// </summary>
//        protected override void OnClosed(EventArgs e)
//        {
//            base.OnClosed(e);
//            s_bl.Volunteer.RemoveObserver(Volunteer.Id, _refreshAction);
//            App.IsAdminLoggedIn = false;
//            s_bl.Volunteer.LogoutVolunteer(Volunteer.Id);
//        }

//        /// <summary>
//        /// Called when user clicks "Update". Sends updated volunteer data to BL.
//        /// </summary>
//        private void btnUpdate_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(Volunteer.Password))
//                    Volunteer.Password = null;

//                s_bl.Volunteer.UpdateVolunteerDetails(Volunteer.Id, Volunteer);
//                MessageBox.Show("Volunteer details updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
//                Close();
//            }
//            catch
//            {
//                MessageBox.Show("Error updating volunteer. Please check inputs and try again.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        /// <summary>
//        /// Called when the volunteer clicks "Finish Call".
//        /// Marks the current call as completed.
//        /// </summary>
//        private void btnFinishCall_Click(object sender, RoutedEventArgs e)
//        {
//            if (Volunteer?.CallInProgress == null) return;

//            try
//            {
//                s_bl.Call.MarkCallCompletion(Volunteer.Id, Volunteer.CallInProgress.Id);
//                MessageBox.Show("Call successfully marked as completed.", "Call Completion", MessageBoxButton.OK, MessageBoxImage.Information);
//                RefreshVolunteer();
//            }
//            catch
//            {
//                MessageBox.Show("Failed to complete the call. Try again later.", "Call Completion Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        /// <summary>
//        /// Called when the volunteer clicks "Cancel Call".
//        /// Cancels the current assignment.
//        /// </summary>
//        private void btnCancelCall_Click(object sender, RoutedEventArgs e)
//        {
//            if (Volunteer?.CallInProgress == null) return;

//            try
//            {
//                s_bl.Call.MarkCallCancellation(Volunteer.Id, Volunteer.CallInProgress.Id);
//                MessageBox.Show("Call successfully marked as cancelled.", "Cancel Call", MessageBoxButton.OK, MessageBoxImage.Information);
//                RefreshVolunteer();
//            }
//            catch
//            {
//                MessageBox.Show("Failed to cancel the call. Try again later.", "Cancel Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        /// <summary>
//        /// Opens the call history window for the current volunteer.
//        /// Prevents opening multiple instances.
//        /// </summary>
//        private void HistoryButton_Click(object sender, RoutedEventArgs e)
//        {
//            int id = Volunteer.Id;
//            if (_openHistoryWindows.Contains(id))
//            {
//                MessageBox.Show("Call history window is already open.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
//                return;
//            }

//            _openHistoryWindows.Add(id);

//            var historyWindow = new VolunteerCallHistoryWindow(id);
//            historyWindow.Closed += (_, _) => _openHistoryWindows.Remove(id);
//            historyWindow.Show();
//        }

//        /// <summary>
//        /// Opens the call selection window for the current volunteer.
//        /// </summary>
//        private void OpenCallsButton_Click(object sender, RoutedEventArgs e)
//        {
//            if (Volunteer == null || Volunteer.Id == 0)
//            {
//                MessageBox.Show("Volunteer details are missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                return;
//            }

//            if (!Volunteer.MaxDistance.HasValue)
//            {
//                MessageBox.Show("Please set maximum distance before selecting a call.", "Distance Required", MessageBoxButton.OK, MessageBoxImage.Warning);
//                return;
//            }

//            try
//            {
//                var selectCallWindow = new SelectCallForTreatmentWindow(
//                    Volunteer.Id,
//                    Volunteer.FullAddress ?? string.Empty,
//                    Volunteer.MaxDistance.Value,
//                    Volunteer
//                );

//                bool? result = selectCallWindow.ShowDialog();
//                if (result == true)
//                    RefreshVolunteer();
//            }
//            catch
//            {
//                MessageBox.Show("Failed to open call selection window. Try again later.", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        /// <summary>
//        /// INotifyPropertyChanged implementation.
//        /// </summary>
//        public event PropertyChangedEventHandler? PropertyChanged;
//        protected void OnPropertyChanged(string propertyName) =>
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//    }
//}
using BlApi;
using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;

namespace PL.Volunteer
{
    public partial class VolunteerSelfWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        private volatile DispatcherOperation? _observerOperation = null; // For Stage 7
        private readonly Action _refreshAction;
        private static readonly HashSet<int> _openHistoryWindows = new();

        /// <summary>
        /// The volunteer object bound to the UI.
        /// </summary>
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

        /// <summary>
        /// Available distance types for ComboBox binding.
        /// </summary>
        public IEnumerable<BO.Enums.DistanceTypes> DistanceTypes =>
            Enum.GetValues(typeof(BO.Enums.DistanceTypes)) as BO.Enums.DistanceTypes[];

        public bool HasCallInProgress => Volunteer?.CallInProgress != null;
        public bool CanSelectCall => Volunteer?.CallInProgress == null && Volunteer?.IsActive == true;
        public bool CanSetInactive => Volunteer?.CallInProgress == null;

        /// <summary>
        /// Constructor that loads volunteer details and sets observer.
        /// </summary>
        public VolunteerSelfWindow(int volunteerId)
        {
            InitializeComponent();
            Volunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId);
            Volunteer.Password = string.Empty;

            _refreshAction = VolunteerObserver;
            s_bl.Volunteer.AddObserver(Volunteer.Id, _refreshAction);
            DataContext = this;
        }

        /// <summary>
        /// מתודת השקפה: מריצה RefreshVolunteer רק דרך DispatcherOperation
        /// </summary>
        private void VolunteerObserver()
        {
            Dispatcher.Invoke(RefreshVolunteer);
        }


        /// <summary>
        /// Refreshes the volunteer's data from the BL.
        /// </summary>
        //private void RefreshVolunteer()
        //{
        //    if (_observerOperation is null || _observerOperation.Status == DispatcherOperationStatus.Completed)
        //    {
        //        _observerOperation = Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
        //            Volunteer.Password = string.Empty;
        //        }));
        //    }
        //}
        private void RefreshVolunteer()
        {
            Volunteer = s_bl.Volunteer.GetVolunteerDetails(Volunteer.Id);
            Volunteer.Password = string.Empty;
        }

        /// <summary>
        /// Called when window is closed; removes the observer from BL.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            s_bl.Volunteer.RemoveObserver(Volunteer.Id, _refreshAction);
            App.IsAdminLoggedIn = false;
            s_bl.Volunteer.LogoutVolunteer(Volunteer.Id);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Volunteer.Password))
                    Volunteer.Password = null;

                s_bl.Volunteer.UpdateVolunteerDetails(Volunteer.Id, Volunteer);
                MessageBox.Show("Volunteer details updated successfully.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch
            {
                MessageBox.Show("Error updating volunteer. Please check inputs and try again.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnFinishCall_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer?.CallInProgress == null) return;

            try
            {
                s_bl.Call.MarkCallCompletion(Volunteer.Id, Volunteer.CallInProgress.Id);
                MessageBox.Show("Call successfully marked as completed.", "Call Completion", MessageBoxButton.OK, MessageBoxImage.Information);
                VolunteerObserver();
            }
            catch
            {
                MessageBox.Show("Failed to complete the call. Try again later.", "Call Completion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelCall_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer?.CallInProgress == null) return;

            try
            {
                s_bl.Call.MarkCallCancellation(Volunteer.Id, Volunteer.CallInProgress.Id);
                MessageBox.Show("Call successfully marked as cancelled.", "Cancel Call", MessageBoxButton.OK, MessageBoxImage.Information);
                VolunteerObserver();
            }
            catch
            {
                MessageBox.Show("Failed to cancel the call. Try again later.", "Cancel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            int id = Volunteer.Id;
            if (_openHistoryWindows.Contains(id))
            {
                MessageBox.Show("Call history window is already open.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _openHistoryWindows.Add(id);

            var historyWindow = new VolunteerCallHistoryWindow(id);
            historyWindow.Closed += (_, _) => _openHistoryWindows.Remove(id);
            historyWindow.Show();
        }

        private void OpenCallsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Volunteer == null || Volunteer.Id == 0)
            {
                MessageBox.Show("Volunteer details are missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Volunteer.MaxDistance.HasValue)
            {
                MessageBox.Show("Please set maximum distance before selecting a call.", "Distance Required", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                if (result == true) ;
                    //VolunteerObserver();
            }
            catch
            {
                MessageBox.Show("Failed to open call selection window. Try again later.", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
