using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlApi;
using BO;
using static BO.Enums;

namespace PL
{
    public partial class CallManagementWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get();

        public CallManagementWindow()
        {
            InitializeComponent();
            QueryCallList();
        }

        public static readonly DependencyProperty CallListProperty =
            DependencyProperty.Register(nameof(CallList), typeof(IEnumerable<CallInList>), typeof(CallManagementWindow), new PropertyMetadata(null));

        public IEnumerable<CallInList> CallList
        {
            get => (IEnumerable<CallInList>)GetValue(CallListProperty);
            set => SetValue(CallListProperty, value);
        }

        public CallType SelectedCallType { get; set; } = CallType.none;

        public CallInList? SelectedCall { get; set; }

        private void QueryCallList()
        {
            try
            {
                CallList = (SelectedCallType == CallType.none)
                    ? s_bl.Call.GetCallsList()
                    : s_bl.Call.GetCallsList(CallInListFields.CallType, SelectedCallType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCallList(CallType? filterCallType = null)
        {
            try
            {
                CallList = s_bl.Call.GetCallsList(CallInListFields.CallType, filterCallType);
                if (!CallList?.Any() ?? true)
                {
                    MessageBox.Show("No calls found for the selected type.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is CallType selectedType)
            {
                SelectedCallType = selectedType;
                QueryCallList();
            }
        }

        private void DeleteCall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CallInList call)
            {
                if (call.CallStatus != CallStatus.opened || call.TotalAssignments > 0)
                {
                    MessageBox.Show("This call cannot be deleted. Only open calls with no assignments can be deleted.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this call?", "Confirm Deletion",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Call.DeleteCall(call.Id!.Value);
                        QueryCallList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        //private void CancelAssignment_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button btn && btn.DataContext is CallInList call)
        //    {
        //        if (!call.CanCancelAssignment)
        //        {
        //            MessageBox.Show("לא ניתן לבטל הקצאה. רק קריאה בטיפול ניתנת לביטול הקצאה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        //            return;
        //        }

        //        if (MessageBox.Show("האם לבטל את ההקצאה עבור קריאה זו?", "אישור פעולה", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //        {
        //            try
        //            {
        //                s_bl.Assignment.CancelAssignmentForCall(call.Id!.Value);
        //                QueryCallList(); // רענון טבלה
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"שגיאה בביטול ההקצאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //    }
        //}

        public IEnumerable<CallType> CallTypeCollection =>
    Enum.GetValues(typeof(CallType)).Cast<CallType>();


        private void AddCall_Click(object sender, RoutedEventArgs e)
        {
            var addCallWindow = new AddCallWindow
            {
                Owner = this
            };

            if (addCallWindow.ShowDialog() == true)
            {
                QueryCallList(); // רענון הרשימה אחרי הוספה
            }
        }
    }
}