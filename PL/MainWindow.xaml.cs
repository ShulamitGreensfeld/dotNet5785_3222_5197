using PL.Volunteer;
using System;
using System.Windows;

namespace PL
{
    public partial class MainWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty RiskRangeProperty =
            DependencyProperty.Register("RiskRange", typeof(int), typeof(MainWindow));

        public int RiskRange
        {
            get { return (int)GetValue(RiskRangeProperty); }
            set { SetValue(RiskRangeProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
            Loaded += MainWindow_Loaded;
        }

        private void btnAdvanceMinute_Click(object sender, RoutedEventArgs e) => s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Minute);
        private void btnAdvanceHour_Click(object sender, RoutedEventArgs e) => s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Hour);
        private void btnAdvanceDay_Click(object sender, RoutedEventArgs e) => s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Day);
        private void btnAdvanceMonth_Click(object sender, RoutedEventArgs e) => s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Month);
        private void btnAdvanceYear_Click(object sender, RoutedEventArgs e) => s_bl.Admin.PromoteClock(BO.Enums.TimeUnit.Year);

        private void btnUpdateRiskRange_Click(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.SetRiskTimeRange(TimeSpan.FromDays(RiskRange * 365));
        }

        private void clockObserver() => Dispatcher.Invoke(() => CurrentTime = s_bl.Admin.GetClock());
        private void configObserver() => RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTime = s_bl.Admin.GetClock();
            RiskRange = (int)s_bl.Admin.GetRiskTimeRange().TotalDays / 365;
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        private void btnVolunteers_Click(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show();
        }
    }
}
