using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BlApi;
using BO;
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
            OpeningTimePicker.SelectedDate = DateTime.Now;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var newCall = new Call
                //{
                //    CallType = (CallType)CallTypeComboBox.SelectedItem,
                //    Opening_time = OpeningTimePicker.SelectedDate ?? DateTime.Now,
                //    CallStatus = CallStatus.opened
                //};

                //s_bl.Call.AddCall(newCall);
                //DialogResult = true;
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