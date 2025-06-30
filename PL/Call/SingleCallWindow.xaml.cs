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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // The call displayed in the window
        private BO.Call _call = new();
        public BO.Call Call
        {
            get => _call;
            set { _call = value; OnPropertyChanged(nameof(Call)); }
        }

        // Available call types
        public IEnumerable<BO.Enums.CallType> CallTypes => Enum.GetValues(typeof(BO.Enums.CallType)) as BO.Enums.CallType[];

        // Distance to call
        private double _callDistance;
        public double CallDistance
        {
            get => _callDistance;
            set { _callDistance = value; OnPropertyChanged(nameof(CallDistance)); }
        }

        // View-only mode
        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
        }

        // Bindable computed properties for UI
        public bool IsCallTypeEditable => !IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsDescriptionReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsAddressReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsMaxFinishTimeReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk);
        public bool IsUpdateEnabled => !IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk);

        // Constructor for call by ID (default editable)
        public SingleCallWindow(int callId)
        {
            InitializeComponent();
            Call = s_bl.Call.GetCallDetails(callId);
            CallDistance = 0;
            IsReadOnly = false;
            DataContext = this;
        }

        // Constructor for already loaded call
        public SingleCallWindow(BO.Call call, double callDistance, bool isReadOnly)
        {
            InitializeComponent();
            Call = call;
            CallDistance = callDistance;
            IsReadOnly = isReadOnly;
            DataContext = this;
        }

        // Update button click → passes call to BL; validation inside BL
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly) return;

            try
            {
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