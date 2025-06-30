using System.Configuration;
using System.Data;
using System.Windows;

namespace PL
{
    /// <summary>
    /// Main WPF application class responsible for application-wide settings.
    /// </summary>
    public partial class App : Application
    {
        // Global flag to track if an admin user is logged in.
        public static bool IsAdminLoggedIn { get; set; }

        public static int LoggedAdminId { get; set; }
    }
}