using System;
using System.Windows;
using FrozenWorld.Models;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class EditItemWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly InventoryItem _item;

        public EditItemWindow(ApiService apiService, InventoryItem item)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _item = item ?? throw new ArgumentNullException(nameof(item));

            PreloadItemData();
        }

        private void PreloadItemData()
        {
            txtItemId.Text = $"Item ID: #{_item.Id}";
            txtName.Text = _item.Name;
            txtCategory.Text = _item.Category;
            txtPrice.Text = _item.PriceString;
            txtQuantity.Text = _item.Quantity.ToString();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                ShowError("Flavor Name and Price are required.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out _))
            {
                ShowError("Please enter a valid numeric Price.");
                txtPrice.Focus();
                return;
            }

            btnSave.IsEnabled = false;
            btnCancel.IsEnabled = false;
            loadingOverlay.Visibility = Visibility.Visible;

            try
            {
                var updatedItem = new InventoryItem
                {
                    Id = _item.Id,
                    Name = txtName.Text.Trim(),
                    Category = string.IsNullOrWhiteSpace(txtCategory.Text) ? "General" : txtCategory.Text.Trim(),
                    PriceString = txtPrice.Text.Trim(),
                    Quantity = int.TryParse(txtQuantity.Text.Trim(), out int q) ? q : 0
                };

                bool success = await _apiService.UpdateItemAsync(_item.Id, updatedItem);

                if (success)
                {
                    _item.Name = updatedItem.Name;
                    _item.Category = updatedItem.Category;
                    _item.PriceString = updatedItem.PriceString;
                    _item.Quantity = updatedItem.Quantity;

                    ModernDialog.ShowSuccess($"\"{updatedItem.Name}\" was updated successfully!", "Item Updated", this);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowError("Failed to update item. Please check your API/Server connection.");
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
