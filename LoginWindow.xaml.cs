using System;
using System.Windows;
using System.Windows.Input;

namespace SikaiAdmin
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => PasswordBox1.Focus();
        }

        
        private async void PasswordBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await TrySignInAsync();
        }

        private async System.Threading.Tasks.Task TrySignInAsync()
        {
            FormError.Visibility = Visibility.Collapsed;

            string password = PasswordBox1.Password;
            if (password.Length == 0)
            {
                ShowError("Enter the password.");
                return;
            }

            PasswordBox1.IsEnabled = false;
            try
            {
                AdminAccount admin = await Db.FindFirstAdminAsync();

                if (admin == null)
                {
                    ShowError("No admin account exists yet. Run admin_setup.sql as SIKAI.");
                    return;
                }

                if (!PasswordHasher.Verify(password, admin.PasswordEntry))
                {
                    ShowError("Incorrect password.");
                    PasswordBox1.Clear();
                    return;
                }

                new MainWindow(admin).Show();
                Close();
            }
            catch (Exception ex)
            {
                ShowError(Db.Describe(ex, ""));
            }
            finally
            {
                PasswordBox1.IsEnabled = true;
                PasswordBox1.Focus();
            }
        }

        private void ShowError(string message)
        {
            FormError.Text = message;
            FormError.Visibility = Visibility.Visible;
        }
    }
}
