using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

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
                    FilterVolunteers();
                }
            }
        }

        public ObservableCollection<BO.VolunteerInList> FilteredVolunteers { get; set; }

        public VolunteerListWindow()
        {
            InitializeComponent();
            FilteredVolunteers = new ObservableCollection<BO.VolunteerInList>(GetAllVolunteers());
            DataContext = this;
        }

        private void FilterVolunteers()
        {
            FilteredVolunteers.Clear();
            var volunteers = GetAllVolunteers();
            foreach (var v in SelectedCallType == BO.Enums.CallType.none
                ? volunteers
                : volunteers.Where(v => v.CallType == SelectedCallType))
            {
                FilteredVolunteers.Add(v);
            }
        }

        private IEnumerable<BO.VolunteerInList> GetAllVolunteers()
        {
            return s_bl.Volunteer.GetVolunteersList();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
