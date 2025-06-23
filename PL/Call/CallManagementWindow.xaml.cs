using BlApi;
using BO;
using DO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PL
{
    public partial class CallManagementWindow : Window, INotifyPropertyChanged
    {
        private static readonly IBl s_bl = Factory.Get();

        public CallManagementWindow()
        {
            InitializeComponent();
            DataContext = this;
            UpdateCallList();
        }

        // אוספים ל-ComboBox-ים
        public IEnumerable<BO.Enums.CallType> CallTypeCollection =>
            Enum.GetValues(typeof(BO.Enums.CallType)).Cast<BO.Enums.CallType>();

        public IEnumerable<BO.Enums.CallStatus> CallStatusCollection =>
            Enum.GetValues(typeof(BO.Enums.CallStatus)).Cast<BO.Enums.CallStatus>();

        public IEnumerable<string> SortFields => new[] { "Opening_time", "CallType", "CallStatus", "TotalAssignments" };
        public IEnumerable<string> GroupFields => new[] { "None", "CallType", "CallStatus" };

        // תכונות Binding
        private BO.Enums.CallType _selectedCallType = BO.Enums.CallType.none;
        public BO.Enums.CallType SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    OnPropertyChanged(nameof(SelectedCallType));
                    UpdateCallList();
                }
            }
        }

        private BO.Enums.CallStatus? _selectedCallStatus = null;
        public BO.Enums.CallStatus? SelectedCallStatus
        {
            get => _selectedCallStatus;
            set
            {
                if (_selectedCallStatus != value)
                {
                    _selectedCallStatus = value;
                    OnPropertyChanged(nameof(SelectedCallStatus));
                    UpdateCallList();
                }
            }
        }

        private string _selectedSortField = "Opening_time";
        public string SelectedSortField
        {
            get => _selectedSortField;
            set
            {
                if (_selectedSortField != value)
                {
                    _selectedSortField = value;
                    OnPropertyChanged(nameof(SelectedSortField));
                    UpdateCallList();
                }
            }
        }

        private string _selectedGroupField = "None";
        public string SelectedGroupField
        {
            get => _selectedGroupField;
            set
            {
                if (_selectedGroupField != value)
                {
                    _selectedGroupField = value;
                    OnPropertyChanged(nameof(SelectedGroupField));
                    UpdateCallList();
                }
            }
        }

        // הרשימה בפועל
        private IEnumerable<CallInList> _callList;
        public IEnumerable<CallInList> CallList
        {
            get => _callList;
            set
            {
                _callList = value;
                OnPropertyChanged(nameof(CallList));
            }
        }

        private ListCollectionView _callListView;
        public ListCollectionView CallListView
        {
            get => _callListView;
            set
            {
                _callListView = value;
                OnPropertyChanged(nameof(CallListView));
            }
        }

        public CallInList? SelectedCall { get; set; }



        // עדכון הרשימה לפי סינון/מיון/קיבוץ
        private void UpdateCallList()
        {
            // קביעת ערכי הסינון והמיון
            BO.Enums.CallInListFields? filterField = null;
            object? filterValue = null;

            if (SelectedCallType != BO.Enums.CallType.none)
            {
                filterField = BO.Enums.CallInListFields.CallType;
                filterValue = SelectedCallType;
            }
            else if (SelectedCallStatus != null)
            {
                filterField = BO.Enums.CallInListFields.CallStatus;
                filterValue = SelectedCallStatus;
            }

            BO.Enums.CallInListFields? sortField = SelectedSortField switch
            {
                "CallType" => BO.Enums.CallInListFields.CallType,
                "CallStatus" => BO.Enums.CallInListFields.CallStatus,
                "TotalAssignments" => BO.Enums.CallInListFields.TotalAssignments,
                _ => BO.Enums.CallInListFields.Opening_time
            };

            // שליפת הקריאות מה-BL עם סינון ומיון
            var list = s_bl.Call.GetCallsList(filterField, filterValue, sortField);

            CallList = list.ToList();

            // קיבוץ (אם צריך)
            var lcv = new ListCollectionView(CallList.ToList());
            lcv.GroupDescriptions.Clear();
            if (SelectedGroupField == "CallType")
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("CallType"));
            else if (SelectedGroupField == "CallStatus")
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("CallStatus"));

            CallListView = lcv;
        }

        // אירוע ל-ComboBox-ים (אם לא רוצים AutoUpdate דרך Setter)
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateCallList();
        }

        private void DeleteCall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CallInList call)
            {
                if (call.CallStatus != BO.Enums.CallStatus.opened || call.TotalAssignments > 0)
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
                        s_bl.Call.DeleteCall(call.CallId);
                        UpdateCallList();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting the call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddCall_Click(object sender, RoutedEventArgs e)
        {
            var addCallWindow = new AddCallWindow
            {
                Owner = this
            };

            if (addCallWindow.ShowDialog() == true)
            {
                UpdateCallList(); // רענון הרשימה אחרי הוספה
            }
        }
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCall != null)
            {
                var window = new PL.Call.SingleCallWindow(SelectedCall.CallId);
                window.ShowDialog();
                UpdateCallList(); // רענון הרשימה לאחר סגירת החלון
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void CancelAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CallInList call)
            {
                // בדיקה: רק קריאה בסטטוס בטיפול עם מתנדב מוקצה
                if (call.CallStatus != BO.Enums.CallStatus.is_treated || string.IsNullOrEmpty(call.LastVolunteerName))
                {
                    MessageBox.Show("ניתן לבטל הקצאה רק לקריאה בסטטוס 'בטיפול' עם מתנדב מוקצה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int? currentUserId = App.CurrentUserId;
                int? assignmentId = call.Id;
                if (currentUserId == null || assignmentId == null)
                {
                    MessageBox.Show("חסר מזהה משתמש או מזהה קריאה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show("האם לבטל את ההקצאה עבור קריאה זו? תישלח הודעה למתנדב.", "אישור פעולה", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Call.MarkCallCancellation(currentUserId.Value, assignmentId.Value);

                        UpdateCallList(); // רענון טבלה
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"שגיאה בביטול ההקצאה: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}