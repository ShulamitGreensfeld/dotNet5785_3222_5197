using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using BlApi;
using BO;

namespace PL.Call
{
    /// <summary>
    /// Interaction logic for displaying and editing a single call
    /// </summary>
    public partial class SingleCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        // The call object displayed and edited in the window
        public BO.Call Call { get; set; }

        // List of available call types
        public IEnumerable<BO.Enums.CallType> CallTypes => Enum.GetValues(typeof(BO.Enums.CallType)) as BO.Enums.CallType[];

        // Distance from volunteer to the call location
        public double CallDistance { get; set; }

        // Indicates whether the entire window is in read-only mode
        public bool IsReadOnly { get; set; }

        // UI bindings for edit permissions
        public bool IsCallTypeEditable => !IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsDescriptionReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsAddressReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsMaxFinishTimeReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk);
        public bool IsUpdateEnabled => !IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk);

        // Notify UI of property changes
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Constructor for read-only view (e.g., when opened by a manager)
        public SingleCallWindow(BO.Call call, double callDistance, bool isReadOnly)
        {
            InitializeComponent();
            Call = call;
            CallDistance = callDistance;
            IsReadOnly = isReadOnly;
            DataContext = this;
        }

        // Alternate constructor to load a call by its ID
        public SingleCallWindow(int callId)
        {
            InitializeComponent();
            Call = s_bl.Call.GetCallDetails(callId);
            CallDistance = 0;
            IsReadOnly = false;
            DataContext = this;
        }

        // Called when the "Update" button is clicked
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(Call.Verbal_description))
                {
                    MessageBox.Show("Please enter a description for the call.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update call details via BL
                s_bl.Call.UpdateCallDetails(Call);

                MessageBox.Show("Call was successfully updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating call: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
