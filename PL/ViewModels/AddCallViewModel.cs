using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BlApi;
using BO;

namespace PL.ViewModels
{
    public class AddCallViewModel : INotifyPropertyChanged
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

        public DateTime OpeningDate { get; }
        private DateTime _maxFinishDate;
        public DateTime MaxFinishDate
        {
            get => _maxFinishDate;
            set { _maxFinishDate = value; OnPropertyChanged(nameof(MaxFinishDate)); }
        }

        public AddCallViewModel()
        {
            OpeningDate = s_bl.Admin.GetClock().Date;
            MaxFinishDate = OpeningDate;
        }

        public BO.Call CreateCall()
        {
            return new BO.Call
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
        }
    }
}