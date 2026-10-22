using System;
using System.Windows;
using FrozenWorld.Models;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class AddItemWindow : Window
    {
        private readonly ApiService _apiService;

        public AddItemWindow(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            // Client-side validation
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowError("Flavor name is required.");
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                ShowError("Price is required.");
                txtPrice.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out _))
            {
                ShowError("Please enter a valid numeric Price (e.g. 450.00).");
                txtPrice.Focus();
                return;
            }

            // Lock controls and present modern loading overlay
            btnSave.IsEnabled = false;
            btnCancel.IsEnabled = false;
            loadingOverlay.Visibility = Visibility.Visible;

            try
            {
                var newItem = new InventoryItem
                {
                    Name = txtName.Text.Trim(),
                    Category = string.IsNullOrWhiteSpace(txtCategory.Text) ? "General" : txtCategory.Text.Trim(),
                    PriceString = txtPrice.Text.Trim(),
                    Quantity = int.TryParse(txtQuantity.Text.Trim(), out int q) ? q : 0
                };

                // Asynchronously submit data to the API
                bool success = await _apiService.CreateItemAsync(newItem);

                if (success)
                {
                    ModernDialog.ShowSuccess($"\"{newItem.Name}\" was successfully added to the catalog!", "Item Created", this);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowError("Failed to save item. Please verify that the API server is running.");
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