using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using BlApi;
using BO;

namespace PL
{
    public partial class AddCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerable<Enums.CallType> CallTypeCollection =>
            Enum.GetValues(typeof(Enums.CallType)).Cast<Enums.CallType>();

        private Enums.CallType _selectedCallType;
        public Enums.CallType SelectedCallType
        {
            get => _selectedCallType;
            set { _selectedCallType = value; OnPropertyChanged(nameof(SelectedCallType)); }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        private string? _fullAddress;
        public string? FullAddress
        {
            get => _fullAddress;
            set { _fullAddress = value; OnPropertyChanged(nameof(FullAddress)); }
        }

        private DateTime _openingDate;
        public DateTime OpeningDate
        {
            get => _openingDate;
            set { _openingDate = value; OnPropertyChanged(nameof(OpeningDate)); }
        }

        private DateTime _maxFinishDate;
        public DateTime MaxFinishDate
        {
            get => _maxFinishDate;
            set { _maxFinishDate = value; OnPropertyChanged(nameof(MaxFinishDate)); }
        }

        public AddCallWindow()
        {
            InitializeComponent();
            OpeningDate = s_bl.Admin.GetClock().Date;
            MaxFinishDate = OpeningDate;
            //DataContext = this;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newCall = new BO.Call
                {
                    CallType = SelectedCallType,
                    Verbal_description = Description,
                    FullAddress = FullAddress,
                    Latitude = null,
                    Longitude = null,
                    Opening_time = s_bl.Admin.GetClock(),
                    Max_finish_time = MaxFinishDate,
                    CallStatus = Enums.CallStatus.opened
                };

                s_bl.Call.AddCall(newCall);
                MessageBox.Show("הקריאה נוספה בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה בעת הוספת הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

//using System;
//using System.Windows;
//using BlApi;
//using PL.ViewModels;

//namespace PL
//{
//    public partial class AddCallWindow : Window
//    {
//        private static readonly IBl s_bl = Factory.Get();
//        private readonly AddCallViewModel _vm;

//        public AddCallWindow()
//        {
//            InitializeComponent();
//            _vm = new AddCallViewModel();
//            DataContext = _vm; // לא Self
//        }

//        private void AddButton_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                var newCall = _vm.CreateCall();
//                s_bl.Call.AddCall(newCall);
//                MessageBox.Show("הקריאה נוספה בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
//                DialogResult = true;
//                Close();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"אירעה שגיאה בעת הוספת הקריאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void CancelButton_Click(object sender, RoutedEventArgs e)
//        {
//            DialogResult = false;
//        }
//    }
//}