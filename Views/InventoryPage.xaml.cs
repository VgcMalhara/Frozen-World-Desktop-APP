using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FrozenWorld.Models;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class InventoryPage : Page
    {
        private readonly ApiService _apiService;
        private List<InventoryItem> _allItems = new();

        public InventoryPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Asynchronously load items when the page finishes loading
            Loaded += async (s, e) => await LoadItemsAsync();
        }

        private bool IsAdminOrOwner()
        {
            string role = _apiService.CurrentUser?.Role?.Trim().ToLower() ?? string.Empty;
            return role == "admin" || role == "owner";
        }

        private async Task LoadItemsAsync()
        {
            ShowLoading("Loading inventory...");

            try
            {
                _allItems = await _apiService.GetItemsAsync() ?? new List<InventoryItem>();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                ModernDialog.ShowError($"Failed to load inventory: {ex.Message}", "Connection Error");
                _allItems = new List<InventoryItem>();
                ApplySearchFilter();
            }
            finally
            {
                HideLoading();
            }
        }

        /// <summary>
        /// Real-time live search filter.
        /// Filters cached items by name, category, ID, or price.
        /// </summary>
        private void ApplySearchFilter()
        {
            string query = txtSearch.Text?.Trim() ?? string.Empty;

            // Total catalog in database is empty
            if (_allItems.Count == 0)
            {
                GridItems.Visibility = Visibility.Collapsed;
                panelEmptyState.Visibility = Visibility.Visible;
                panelSearchEmptyState.Visibility = Visibility.Collapsed;
                txtSearchResults.Text = "No items in catalog";
                btnClearSearch.Visibility = Visibility.Collapsed;
                return;
            }

            panelEmptyState.Visibility = Visibility.Collapsed;

            List<InventoryItem> filtered;
            if (string.IsNullOrWhiteSpace(query))
            {
                filtered = _allItems;
                btnClearSearch.Visibility = Visibility.Collapsed;
                txtSearchResults.Text = $"Showing all {_allItems.Count} flavors";
            }
            else
            {
                btnClearSearch.Visibility = Visibility.Visible;
                filtered = _allItems.Where(item =>
                    (!string.IsNullOrEmpty(item.Name) && item.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(item.Category) && item.Category.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    item.Id.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(item.PriceString) && item.PriceString.Contains(query, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                txtSearchResults.Text = $"Showing {filtered.Count} of {_allItems.Count} matching flavors";
            }

            // Search query returned no matches
            if (filtered.Count == 0)
            {
                GridItems.Visibility = Visibility.Collapsed;
                panelSearchEmptyState.Visibility = Visibility.Visible;
                txtSearchEmptyMsg.Text = $"No flavors found matching \"{query}\". Try searching for another keyword or clear the search.";
            }
            else
            {
                GridItems.Visibility = Visibility.Visible;
                panelSearchEmptyState.Visibility = Visibility.Collapsed;
                GridItems.ItemsSource = filtered;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            txtSearch.Focus();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadItemsAsync();
        }

        private async void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            string role = _apiService.CurrentUser?.Role?.Trim().ToLower() ?? string.Empty;

            // RBAC Check: Cashiers cannot add inventory items (Only Owner/Admin and Manager are permitted)
            if (role == "cashier" || (!IsAdminOrOwner() && role != "manager"))
            {
                MessageBox.Show("Access Denied: You do not have permission to perform this action.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            btnAddItem.IsEnabled = false;

            try
            {
                var addWindow = new AddItemWindow(_apiService);
                addWindow.Owner = Window.GetWindow(this);
                bool? result = addWindow.ShowDialog();

                if (result == true)
                {
                    await LoadItemsAsync();
                }
            }
            finally
            {
                btnAddItem.IsEnabled = true;
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            string role = _apiService.CurrentUser?.Role?.Trim().ToLower() ?? string.Empty;

            // RBAC Check: Verify user role has permission to edit (Owner, Manager, Cashier are all authorized)
            if (role != "admin" && role != "owner" && role != "manager" && role != "cashier")
            {
                MessageBox.Show("Access Denied: You do not have permission to perform this action.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (sender is Button editBtn && editBtn.DataContext is InventoryItem item)
            {
                editBtn.IsEnabled = false;

                try
                {
                    var editWindow = new EditItemWindow(_apiService, item);
                    editWindow.Owner = Window.GetWindow(this);
                    bool? result = editWindow.ShowDialog();

                    if (result == true)
                    {
                        await LoadItemsAsync();
                    }
                }
                finally
                {
                    editBtn.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Handles deleting an inventory item with Role-Based Access Control (RBAC).
        /// Business Rules:
        /// - Admin / Owner: Permitted.
        /// - Manager / Cashier: Blocked with Access Denied message.
        /// </summary>
        private async void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminOrOwner())
            {
                MessageBox.Show("Access Denied: You do not have permission to perform this action.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (sender is Button deleteBtn && deleteBtn.DataContext is InventoryItem item)
            {
                bool confirmed = ModernDialog.ShowConfirm(
                    $"Are you sure you want to permanently delete \"{item.Name}\" (ID: #{item.Id}) from your catalog?",
                    title: "Delete Flavor",
                    confirmText: "Delete Flavor",
                    cancelText: "Keep Item",
                    isDestructive: true);

                if (!confirmed) return;

                ShowLoading($"Deleting \"{item.Name}\"...");

                try
                {
                    bool success = await _apiService.DeleteItemAsync(item.Id);

                    if (success)
                    {
                        ModernDialog.ShowSuccess($"\"{item.Name}\" was successfully deleted.", "Item Deleted");
                        await LoadItemsAsync();
                    }
                    else
                    {
                        ModernDialog.ShowError("Failed to delete inventory item. Please check server logs.", "Deletion Failed");
                    }
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowError($"An unexpected error occurred: {ex.Message}", "Error");
                }
                finally
                {
                    HideLoading();
                }
            }
        }

        // Backward compatibility handler
        private void BtnDelete_Click(object sender, RoutedEventArgs e) => BtnDeleteItem_Click(sender, e);

        private void ShowLoading(string message)
        {
            txtLoadingTitle.Text = message;
            loadingOverlay.Visibility = Visibility.Visible;
        }

        private void HideLoading()
        {
            loadingOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
