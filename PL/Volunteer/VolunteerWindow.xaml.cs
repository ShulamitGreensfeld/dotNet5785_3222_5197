using System.Windows;
using BlApi;

namespace PL.Volunteer
{
    public partial class VolunteerWindow : Window
    {
        // BL access object
        private static readonly IBl s_bl = BlApi.Factory.Get();

        // תכונת תלות לאובייקט Volunteer
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }
        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register(nameof(CurrentVolunteer), typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

        // תכונת תלות עבור טקסט הכפתור
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(VolunteerWindow), new PropertyMetadata("Add"));

        // קונסטרקטור כללי עם פרמטר id
        public VolunteerWindow(int id = 0)
        {
            InitializeComponent();
            if (id != 0)
            {
                try
                {
                    CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
                    ButtonText = "Update";
                }
                catch
                {
                    // טיפול בחריגות - אפשר להציג הודעה למשתמש
                    CurrentVolunteer = new BO.Volunteer();
                    ButtonText = "Add";
                }
            }
            else
            {
                CurrentVolunteer = new BO.Volunteer();
                ButtonText = "Add";
            }
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add")
                {
                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    MessageBox.Show("Volunteer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Update
                {
                    // קבל את המתנדב המקורי מה-BL
                    var oldVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer!.Id);

                    // אם המשתמש לא הזין סיסמה חדשה, שמור את הסיסמה הישנה (המוצפנת)
                    if (string.IsNullOrWhiteSpace(CurrentVolunteer.Password))
                        CurrentVolunteer.Password = oldVolunteer.Password;

                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);
                    MessageBox.Show("Volunteer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                //DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Operation failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolunteerObserver()
        {
            if (CurrentVolunteer?.Id != 0)
            {
                int id = CurrentVolunteer.Id;
                CurrentVolunteer = null;
                CurrentVolunteer = s_bl.Volunteer.GetVolunteerDetails(id);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.Id != 0)
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, VolunteerObserver);
        }
        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (CurrentVolunteer?.Id != 0)
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, VolunteerObserver);
        }

    }
}
