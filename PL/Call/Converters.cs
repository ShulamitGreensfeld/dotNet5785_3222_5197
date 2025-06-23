using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BO;

namespace PL
{
    // ����� ����� - ���� �� �� ������ ����� ���� ������
    public class DeleteButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CallInList call)
            {
                return (call.CallStatus == BO.Enums.CallStatus.opened && call.TotalAssignments == 0)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ����� ����� ����� - ���� �� �� ������ ������ ��� �� �����
    public class CancelAssignmentVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CallInList call)
            {
                return (call.CallStatus == BO.Enums.CallStatus.is_treated && !string.IsNullOrEmpty(call.LastVolunteerName))
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}