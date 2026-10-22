using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly SolidColorBrush _activeBrush = new(Color.FromRgb(37, 99, 235)); // #2563EB
        private readonly SolidColorBrush _inactiveBrush = Brushes.Transparent;
        private readonly SolidColorBrush _textActive = Brushes.White;
        private readonly SolidColorBrush _textInactive = new(Color.FromRgb(148, 163, 184)); // #94A3B8

        public MainWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Populate user profile info in the sidebar
            string userName = _apiService.CurrentUser?.Name ?? "User";
            txtUserName.Text = userName;
            txtUserRole.Text = (_apiService.CurrentUser?.Role ?? "Cashier").ToUpper();
            txtAvatarInitial.Text = string.IsNullOrEmpty(userName) ? "U" : userName.Substring(0, 1).ToUpper();

            // Hook up subtle page navigation transition
            MainFrame.Navigated += MainFrame_Navigated;

            // Load Inventory Page as default
            NavigateToInventory();
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Subtle, premium fade-in animation on page change
            if (MainFrame.Content is FrameworkElement pageElement)
            {
                pageElement.Opacity = 0.0;
                var fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(220))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                pageElement.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        private void NavigateToInventory()
        {
            SetActiveNav(btnInventory, btnCustomers);
            MainFrame.Navigate(new InventoryPage(_apiService));
        }

        private void NavigateToCustomers()
        {
            SetActiveNav(btnCustomers, btnInventory);
            MainFrame.Navigate(new CustomerPage(_apiService));
        }

        private void SetActiveNav(System.Windows.Controls.Button activeBtn, System.Windows.Controls.Button inactiveBtn)
        {
            activeBtn.Background = _activeBrush;
            activeBtn.Foreground = _textActive;

            inactiveBtn.Background = _inactiveBrush;
            inactiveBtn.Foreground = _textInactive;
        }

        private void BtnInventory_Click(object sender, RoutedEventArgs e)
        {
            NavigateToInventory();
        }

        private void BtnCustomers_Click(object sender, RoutedEventArgs e)
        {
            NavigateToCustomers();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            bool confirm = ModernDialog.ShowConfirm(
                "Are you sure you want to end your session and log out?",
                title: "Log Out",
                confirmText: "Log Out",
                cancelText: "Stay Logged In",
                isDestructive: false,
                owner: this);

            if (!confirm) return;

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}