//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Windows;
//using BlApi;
//using BO;

//namespace PL.Call
//{
//    public partial class SingleCallWindow : Window, INotifyPropertyChanged
//    {
//        private static readonly IBl s_bl = BlApi.Factory.Get();

//        public BO.Call Call { get; set; }
//        public IEnumerable<BO.Enums.CallType> CallTypes => Enum.GetValues(typeof(BO.Enums.CallType)) as BO.Enums.CallType[];

//        // Properties for UI logic
//        public bool IsCallTypeEditable => Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk;
//        public bool IsDescriptionReadOnly => !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
//        public bool IsAddressReadOnly => !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
//        public bool IsMaxFinishTimeReadOnly => !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk);
//        public bool IsUpdateEnabled => Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk;

//        public event PropertyChangedEventHandler? PropertyChanged;
//        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

//        public SingleCallWindow(int callId)
//        {
//            InitializeComponent();
//            Call = s_bl.Call.GetCallDetails(callId);
//            DataContext = this;
//        }



//        private void UpdateButton_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                // בדיקות פורמט בסיסיות (לדוג' תיאור לא ריק)
//                if (string.IsNullOrWhiteSpace(Call.Verbal_description))
//                {
//                    MessageBox.Show("יש להזין תיאור לקריאה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
//                    return;
//                }
//                // שלח עדכון ל-BL
//                s_bl.Call.UpdateCallDetails(Call);
//                MessageBox.Show("הקריאה עודכנה בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
//                Close();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("שגיאה בעדכון הקריאה: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using BlApi;
using BO;

namespace PL.Call
{
    public partial class SingleCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = BlApi.Factory.Get();

        public BO.Call Call { get; set; }
        public IEnumerable<BO.Enums.CallType> CallTypes => Enum.GetValues(typeof(BO.Enums.CallType)) as BO.Enums.CallType[];

        public double CallDistance { get; set; }
        public bool IsReadOnly { get; set; }

        // Properties for UI logic
        public bool IsCallTypeEditable => !IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsDescriptionReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsAddressReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk);
        public bool IsMaxFinishTimeReadOnly => IsReadOnly || !(Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk);
        public bool IsUpdateEnabled => !IsReadOnly && (Call.CallStatus == BO.Enums.CallStatus.opened || Call.CallStatus == BO.Enums.CallStatus.opened_at_risk || Call.CallStatus == BO.Enums.CallStatus.is_treated || Call.CallStatus == BO.Enums.CallStatus.treated_at_risk);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // קונסטרקטור לצפייה בלבד
        public SingleCallWindow(BO.Call call, double callDistance, bool isReadOnly)
        {
            InitializeComponent();
            Call = call;
            CallDistance = callDistance;
            IsReadOnly = isReadOnly;
            DataContext = this;
        }

        // קונסטרקטור רגיל (אם צריך)
        public SingleCallWindow(int callId)
        {
            InitializeComponent();
            Call = s_bl.Call.GetCallDetails(callId);
            CallDistance = 0;
            IsReadOnly = false;
            DataContext = this;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsReadOnly)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(Call.Verbal_description))
                {
                    MessageBox.Show("יש להזין תיאור לקריאה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                s_bl.Call.UpdateCallDetails(Call);
                MessageBox.Show("הקריאה עודכנה בהצלחה.", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בעדכון הקריאה: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}