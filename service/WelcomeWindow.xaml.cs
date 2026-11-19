using System.Windows;
using System.Windows.Controls;
using service.Models;
using service.Services;
using service.ViewModels;

namespace service
{
    public partial class WelcomeWindow : Window
    {
        private readonly DataService _dataService;
        private readonly BookingService _bookingService;

        public WelcomeWindow()
        {
            InitializeComponent();
            DataContext = this;
            _dataService = new DataService();
            _bookingService = new BookingService(_dataService);
            _bookingService.EnsureSeed();
            UserComboBox.ItemsSource = _bookingService.GetUsers();
        }

        // Фото для экрана приветствия (зал переговоров, как в макете)
        public string HeroImage => DemoImages.RoomA;

        // Переключение панелей
        private void Show(FrameworkElement panel)
        {
            WelcomePanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Collapsed;
            panel.Visibility = Visibility.Visible;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Show(LoginPanel);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Show(RegisterPanel);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Show(WelcomePanel);
        }

        private void DoLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = UserComboBox.SelectedItem as User;
            if (user == null)
            {
                MessageBox.Show("Выберите пользователя.", "Вход", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            OpenMain(user);
        }

        private void DoRegister_Click(object sender, RoutedEventArgs e)
        {
            var fullName = FullNameBox.Text.Trim();
            var email = EmailBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Заполните ФИО и Email.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var user = new User(0, fullName, email, "Employee");
            _bookingService.AddUser(user);
            OpenMain(user);
        }

        private void OpenMain(User user)
        {
            var main = new MainWindow(new MainViewModel(user));
            main.Show();
            Close();
        }
    }
}
