using System;
using System.Windows;
using FrozenWorld.Models;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class AddCustomerWindow : Window
    {
        private readonly ApiService _apiService;

        public AddCustomerWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowError("Customer Name is required.");
                txtName.Focus();
                return;
            }

            // Lock controls and present modern loading overlay
            btnSave.IsEnabled = false;
            btnCancel.IsEnabled = false;
            loadingOverlay.Visibility = Visibility.Visible;

            try
            {
                var newCustomer = new Customer
                {
                    Name = txtName.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim()
                };

                // Call API asynchronously
                var result = await _apiService.CreateCustomerAsync(newCustomer);

                if (result.Success)
                {
                    ModernDialog.ShowSuccess($"Customer \"{newCustomer.Name}\" was registered successfully!", "Customer Added", this);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    // Displays Laravel regex / unique validation errors
                    ShowError(result.ErrorMessage);
                    loadingOverlay.Visibility = Visibility.Collapsed;
                    btnSave.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred: " + ex.Message);
                loadingOverlay.Visibility = Visibility.Collapsed;
                btnSave.IsEnabled = true;
                btnCancel.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            borderError.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            txtError.Text = string.Empty;
            borderError.Visibility = Visibility.Collapsed;
        }
    }
}