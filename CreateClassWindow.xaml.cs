using System;
using System.Windows;

namespace SikaiAdmin
{
    public partial class CreateClassWindow : Window
    {
        public CreateClassWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadTeachersAsync();
        }

        private async System.Threading.Tasks.Task LoadTeachersAsync()
        {
            FormError.Visibility = Visibility.Collapsed;
            try
            {
                var teachers = await Db.ListUsersAsync(Role.Teacher);
                TeacherCombo.ItemsSource = teachers;
                TeacherCombo.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ShowError(Db.Describe(ex, ""));
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadTeachersAsync();
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            FormError.Visibility = Visibility.Collapsed;
            FormSuccess.Visibility = Visibility.Collapsed;

            string className = ClassNameBox.Text.Trim();
            string classCode = ClassCodeBox.Text.Trim();

            if (className.Length == 0 || classCode.Length == 0)
            {
                ShowError("Enter a class name and a class code.");
                return;
            }

            
            var selected = TeacherCombo.SelectedItem as UserRow;
            if (selected == null)
            {
                ShowError("Choose the teacher who will run this class.");
                return;
            }
            long teacherId = selected.Id;

            CreateButton.IsEnabled = false;
            try
            {
                await Db.CreateClassAsync(className, classCode, teacherId);

                FormSuccess.Text = "Class \"" + className + "\" created with code " + classCode + ".";
                FormSuccess.Visibility = Visibility.Visible;

                ClassNameBox.Clear();
                ClassCodeBox.Clear();
                TeacherCombo.SelectedIndex = -1;
                ClassNameBox.Focus();
            }
            catch (Exception ex)
            {
                ShowError(Db.Describe(ex, "That class code is already in use."));
            }
            finally
            {
                CreateButton.IsEnabled = true;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowError(string message)
        {
            FormError.Text = message;
            FormError.Visibility = Visibility.Visible;
        }
    }
}
