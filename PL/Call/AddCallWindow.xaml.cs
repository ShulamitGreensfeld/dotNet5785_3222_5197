using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlApi;
using static BO.Enums;

namespace PL
{
    public partial class AddCallWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get();

        public IEnumerable<CallType> CallTypeCollection =>
            Enum.GetValues(typeof(CallType)).Cast<CallType>();

        public AddCallWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var grid = (Grid)this.Content;

                var stack1 = (StackPanel)grid.Children[0]; // סוג קריאה
                var stack2 = (StackPanel)grid.Children[1]; // תיאור
                var stack3 = (StackPanel)grid.Children[2]; // כתובת
                var stack4 = (StackPanel)grid.Children[3]; // קו רוחב
                var stack5 = (StackPanel)grid.Children[4]; // קו אורך
                var stack6 = (StackPanel)grid.Children[5]; // תאריך פתיחה
                var stack7 = (StackPanel)grid.Children[6]; // תאריך יעד

                var callTypeComboBox = (ComboBox)stack1.Children[1];
                var descriptionTextBox = (TextBox)stack2.Children[1];
                var addressTextBox = (TextBox)stack3.Children[1];
                var latitudeTextBox = (TextBox)stack4.Children[1];
                var longitudeTextBox = (TextBox)stack5.Children[1];
                var openingDatePicker = (DatePicker)stack6.Children[1];
                var maxFinishDatePicker = (DatePicker)stack7.Children[1];

                var newCall = new BO.Call
                {
                    CallType = (CallType)callTypeComboBox.SelectedItem,
                    Verbal_description = descriptionTextBox.Text,
                    FullAddress = addressTextBox.Text,
                    Latitude = double.TryParse(latitudeTextBox.Text, out var lat) ? lat : 0,
                    Longitude = double.TryParse(longitudeTextBox.Text, out var lon) ? lon : 0,
                    Opening_time = openingDatePicker.SelectedDate ?? DateTime.Now,
                    Max_finish_time = maxFinishDatePicker.SelectedDate ?? DateTime.Now,
                    CallStatus = BO.Enums.CallStatus.opened
                };

                s_bl.Call.AddCall(newCall);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}