using BO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PL.Volunteer
{
    public partial class VolunteerListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public VolunteerListWindow()
        {
            InitializeComponent();
            CallTypeCollection = Enum.GetValues(typeof(Enums.CallType)).Cast<Enums.CallType>();
            DataContext = this;
        }

        // Property for CallTypeCollection (not tied to a DependencyProperty)
        public IEnumerable<BO.Enums.CallType> CallTypeCollection { get; private set; }

        // Dependency Property for the Volunteer List
        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
            set { SetValue(VolunteerListProperty, value); }
        }
    }
}
