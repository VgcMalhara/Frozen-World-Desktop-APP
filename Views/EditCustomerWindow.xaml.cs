using System;
using System.Windows;
using FrozenWorld.Models;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class EditCustomerWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly Customer _customer;

        public EditCustomerWindow(ApiService apiService, Customer customer)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));

            PreloadCustomerData();
        }

        private void PreloadCustomerData()
        {
            txtCustomerId.Text = $"Customer ID: #{_customer.Id}";
            txtName.Text = _customer.Name;
            txtEmail.Text = _customer.Email ?? string.Empty;
            txtPhone.Text = _customer.Phone ?? string.Empty;
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
                var updatedCustomer = new Customer
                {
                    Id = _customer.Id,
                    Name = txtName.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim()
                };

                var result = await _apiService.UpdateCustomerAsync(_customer.Id, updatedCustomer);

                if (result.Success)
                {
                    _customer.Name = updatedCustomer.Name;
                    _customer.Email = updatedCustomer.Email;
                    _customer.Phone = updatedCustomer.Phone;

                    ModernDialog.ShowSuccess($"Customer \"{updatedCustomer.Name}\" was updated successfully!", "Changes Saved", this);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
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
