using static BO.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace PL
{
    //public class DeleteButtonVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var call = value as BO.Call;
    //        if (call == null) return Visibility.Collapsed;

    //        bool isOpen = call.CallStatus == CallStatus.opened;
    //        bool hasNoTasks = call.TaskCount == 0;

    //        return isOpen && hasNoTasks ? Visibility.Visible : Visibility.Collapsed;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //        => throw new NotImplementedException();
    //}
}