using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FrozenWorld.Models;
using FrozenWorld.Services;

namespace FrozenWorld.Views
{
    public partial class CustomerPage : Page
    {
        private readonly ApiService _apiService;

        public CustomerPage(ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Load data from API when the page opens
            Loaded += async (s, e) => await LoadCustomersAsync();
        }

        private bool IsAdminOrOwner()
        {
            string role = _apiService.CurrentUser?.Role?.Trim().ToLower() ?? string.Empty;
            return role == "admin" || role == "owner";
        }

        private async Task LoadCustomersAsync()
        {
            ShowLoading("Loading customers...");

            try
            {
                var customers = await _apiService.GetCustomersAsync();
                UpdateGridData(customers);
            }
            catch (Exception ex)
            {
                ModernDialog.ShowError($"Failed to load customer list: {ex.Message}", "Connection Error");
                UpdateGridData(new List<Customer>());
            }
            finally
            {
                HideLoading();
            }
        }

        private void UpdateGridData(List<Customer> customers)
        {
            GridCustomers.ItemsSource = customers;

            if (customers == null || customers.Count == 0)
            {
                GridCustomers.Visibility = Visibility.Collapsed;
                panelEmptyState.Visibility = Visibility.Visible;
            }
            else
            {
                GridCustomers.Visibility = Visibility.Visible;
                panelEmptyState.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            string role = _apiService.CurrentUser?.Role?.Trim().ToLower() ?? string.Empty;

            // RBAC Check: Verify user role has permission to add customers (Owner, Manager, Cashier are all authorized)
            if (role != "admin" && role != "owner" && role != "manager" && role != "cashier")
            {
                MessageBox.Show("Access Denied: You do not have permission to perform this action.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            btnAddCustomer.IsEnabled = false;

            try
            {
                var addWindow = new AddCustomerWindow(_apiService);
                addWindow.Owner = Window.GetWindow(this);
                bool? result = addWindow.ShowDialog();

                if (result == true)
                {
                    await LoadCustomersAsync();
                }
            }
            finally
            {
                btnAddCustomer.IsEnabled = true;
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            string role = _apiService.CurrentUser?.Role?.Trim().ToLower() ?? string.Empty;

            // RBAC Check: Verify user role has permission to edit customers (Owner, Manager, Cashier are all authorized)
            if (role != "admin" && role != "owner" && role != "manager" && role != "cashier")
            {
                MessageBox.Show("Access Denied: You do not have permission to perform this action.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (sender is Button editBtn && editBtn.DataContext is Customer customer)
            {
                editBtn.IsEnabled = false;

                try
                {
                    var editWindow = new EditCustomerWindow(_apiService, customer);
                    editWindow.Owner = Window.GetWindow(this);
                    bool? result = editWindow.ShowDialog();

                    if (result == true)
                    {
                        await LoadCustomersAsync();
                    }
                }
                finally
                {
                    editBtn.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Handles deleting a customer with Role-Based Access Control (RBAC).
        /// Business Rules:
        /// - Admin / Owner: Permitted.
        /// - Manager / Cashier: Blocked with Access Denied message.
        /// </summary>
        private async void BtnDeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminOrOwner())
            {
                MessageBox.Show("Access Denied: You do not have permission to perform this action.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (sender is Button deleteBtn && deleteBtn.DataContext is Customer customer)
            {
                bool confirmed = ModernDialog.ShowConfirm(
                    $"Are you sure you want to permanently delete customer \"{customer.Name}\" (ID: #{customer.Id})?",
                    title: "Delete Customer",
                    confirmText: "Delete Customer",
                    cancelText: "Keep Customer",
                    isDestructive: true);

                if (!confirmed) return;

                ShowLoading($"Deleting \"{customer.Name}\"...");

                try
                {
                    var result = await _apiService.DeleteCustomerAsync(customer.Id);

                    if (result.Success)
                    {
                        ModernDialog.ShowSuccess($"Customer \"{customer.Name}\" was successfully deleted.", "Customer Deleted");
                        await LoadCustomersAsync();
                    }
                    else
                    {
                        ModernDialog.ShowError($"Failed to delete customer:\n{result.ErrorMessage}", "Deletion Failed");
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
        private void BtnDelete_Click(object sender, RoutedEventArgs e) => BtnDeleteCustomer_Click(sender, e);

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
