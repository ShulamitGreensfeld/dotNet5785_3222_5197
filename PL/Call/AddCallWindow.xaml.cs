//using System.Windows;
//using System.Windows.Controls;
//using BlApi;

//namespace PL
//{
//    public partial class AddCallWindow : Window
//    {
//        private static readonly IBl s_bl = Factory.Get();

//        public DateTime SystemClockDate => s_bl.Admin.GetClock().Date;

//        public IEnumerable<BO.Enums.CallType> CallTypeCollection =>
//            Enum.GetValues(typeof(BO.Enums.CallType)).Cast<BO.Enums.CallType>();

//        public AddCallWindow()
//        {
//            InitializeComponent();
//            DataContext = this;
//            openingDatePicker.SelectedDate = SystemClockDate;
//            closingDatePicker.SelectedDate = SystemClockDate;
//            openingDatePicker.IsEnabled = true;
//        }

//        private void AddButton_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                var grid = (Grid)this.Content;

//                if (grid.Children.Count < 6)
//                {
//                    MessageBox.Show("חסרים שדות בתצוגה. לא ניתן להוסיף קריאה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
//                    return;
//                }

//                var stack1 = (StackPanel)grid.Children[0];
//                var stack2 = (StackPanel)grid.Children[1];
//                var stack3 = (StackPanel)grid.Children[2];
//                var stack4 = (StackPanel)grid.Children[3];
//                var stack5 = (StackPanel)grid.Children[4];

//                var callTypeComboBox = (ComboBox)stack1.Children[1];
//                var descriptionTextBox = (TextBox)stack2.Children[1];
//                var addressTextBox = (TextBox)stack3.Children[1];
//                var openingDatePicker = (DatePicker)stack4.Children[1];
//                var maxFinishDatePicker = (DatePicker)stack5.Children[1];

//                var newCall = new BO.Call
//                {
//                    CallType = (BO.Enums.CallType)callTypeComboBox.SelectedItem,
//                    Verbal_description = descriptionTextBox.Text,
//                    FullAddress = addressTextBox.Text,
//                    Latitude = null,
//                    Longitude = null,
//                    Opening_time = s_bl.Admin.GetClock(),
//                    Max_finish_time = maxFinishDatePicker.SelectedDate ?? s_bl.Admin.GetClock(),
//                    CallStatus = BO.Enums.CallStatus.opened
//                };

//                s_bl.Call.AddCall(newCall);
//                DialogResult = true;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error adding call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void CancelButton_Click(object sender, RoutedEventArgs e)
//        {
//            DialogResult = false;
//        }
//    }
//}
// AddCallWindow.xaml.cs - גרסה מותאמת ל-MVVM מלא עם Data Binding מלא וללא x:Name
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
            DataContext = this;
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