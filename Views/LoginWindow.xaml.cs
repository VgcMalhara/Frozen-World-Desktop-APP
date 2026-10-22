using System;
using System.Windows;
using System.Windows.Input;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Allow pressing Enter key directly in Email or Password box to login
            txtEmail.KeyDown += Input_KeyDown;
            txtPassword.KeyDown += Input_KeyDown;
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(btnLogin, new RoutedEventArgs());
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Clear any existing error state
            borderError.Visibility = Visibility.Collapsed;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                txtError.Text = "Please enter both email address and password.";
                borderError.Visibility = Visibility.Visible;
                return;
            }

            // Visual feedback during login
            btnLogin.IsEnabled = false;
            btnLogin.Content = "Signing in...";

            try
            {
                bool success = await _apiService.LoginAsync(email, password);

                if (success)
                {
                    var mainWindow = new MainWindow(_apiService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // Show error and explicitly keep button visible & re-enabled
                    txtError.Text = "Invalid email or password. Please verify your credentials.";
                    borderError.Visibility = Visibility.Visible;

                    ResetLoginButton();
                    txtPassword.Focus();
                    txtPassword.SelectAll();
                }
            }
            catch (Exception ex)
            {
                txtError.Text = "Connection error: " + ex.Message;
                borderError.Visibility = Visibility.Visible;

                ResetLoginButton();
            }
        }

        private void ResetLoginButton()
        {
            btnLogin.Visibility = Visibility.Visible;
            btnLogin.IsEnabled = true;
            btnLogin.Content = "Sign In to Account";
        }
    }
}