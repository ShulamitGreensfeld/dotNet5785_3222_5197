using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using BlApi;
using BO;

namespace PL
{
    /// <summary>
    /// Interaction logic for AddCallWindow.xaml
    /// </summary>
    public partial class AddCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // List of call types for the ComboBox binding
        public IEnumerable<Enums.CallType> CallTypeCollection =>
            Enum.GetValues(typeof(Enums.CallType)).Cast<Enums.CallType>();

        // The selected call type
        private Enums.CallType _selectedCallType;
        public Enums.CallType SelectedCallType
        {
            get => _selectedCallType;
            set { _selectedCallType = value; OnPropertyChanged(nameof(SelectedCallType)); }
        }

        // The description entered by the user
        private string? _description;
        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        // The full address entered by the user
        private string? _fullAddress;
        public string? FullAddress
        {
            get => _fullAddress;
            set { _fullAddress = value; OnPropertyChanged(nameof(FullAddress)); }
        }

        // Automatically set opening date (read-only for user)
        private DateTime _openingDate;
        public DateTime OpeningDate
        {
            get => _openingDate;
            set { _openingDate = value; OnPropertyChanged(nameof(OpeningDate)); }
        }

        // User-selected maximum allowed finish time
        private DateTime _maxFinishDate;
        public DateTime MaxFinishDate
        {
            get => _maxFinishDate;
            set { _maxFinishDate = value; OnPropertyChanged(nameof(MaxFinishDate)); }
        }

        // Constructor: initializes default dates
        public AddCallWindow()
        {
            InitializeComponent();
            OpeningDate = s_bl.Admin.GetClock().Date;
            MaxFinishDate = OpeningDate;
        }

        // Called when the Add button is clicked – attempts to create the new call

                   private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var call = new BO.Call
                {
                    CallType = SelectedCallType,
                    Verbal_description = Description,
                    FullAddress = FullAddress,
                    Max_finish_time = MaxFinishDate,
                    Opening_time = s_bl.Admin.GetClock(),
                    CallStatus = BO.Enums.CallStatus.opened
                };

                s_bl.Call.AddCall(call);

                MessageBox.Show("Call added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Called when the Cancel button is clicked – closes the window
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}