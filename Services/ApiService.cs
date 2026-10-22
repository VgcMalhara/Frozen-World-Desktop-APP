using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FrozenWorld.Models;

namespace FrozenWorld.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public User? CurrentUser { get; private set; }
        public string Token { get; private set; } = string.Empty;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/api/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { email, password };
                var response = await _httpClient.PostAsJsonAsync("auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        Token = result.Token;
                        CurrentUser = result.User;


                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public async Task<List<InventoryItem>> GetItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<InventoryItem>>>("items");
                return response?.Data ?? new List<InventoryItem>();
            }
            catch
            {
                return new List<InventoryItem>();
            }
        }


        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Customer>>>("customers");
                return response?.Data ?? new List<Customer>();
            }
            catch
            {
                return new List<Customer>();
            }
        }


        public async Task<bool> CreateItemAsync(InventoryItem newItem)
        {
            try
            {

                var response = await _httpClient.PostAsJsonAsync("items", newItem);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public async Task<(bool Success, string ErrorMessage)> CreateCustomerAsync(Customer newCustomer)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("customers", newCustomer);

                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }


                string errorMsg = await ExtractErrorMessageAsync(response, "Validation failed. Please check your inputs.");
                return (false, errorMsg);
            }
            catch (Exception ex)
            {
                return (false, "Server connection failed: " + ex.Message);
            }
        }


        public async Task<bool> UpdateItemAsync(int id, InventoryItem updatedItem)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"items/{id}", updatedItem);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"items/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public async Task<(bool Success, string ErrorMessage)> UpdateCustomerAsync(int id, Customer updatedCustomer)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"customers/{id}", updatedCustomer);

                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }


                string errorMsg = await ExtractErrorMessageAsync(response, "Failed to update customer. Please check your inputs.");
                return (false, errorMsg);
            }
            catch (Exception ex)
            {
                return (false, "Server connection failed: " + ex.Message);
            }
        }


        public Task<(bool Success, string ErrorMessage)> UpdateCustomerAsync(Customer customer)
        {
            return UpdateCustomerAsync(customer.Id, customer);
        }


        public async Task<(bool Success, string ErrorMessage)> DeleteCustomerAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"customers/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }

                string errorMsg = await ExtractErrorMessageAsync(response, "Failed to delete customer.");
                return (false, errorMsg);
            }
            catch (Exception ex)
            {
                return (false, "Server connection failed: " + ex.Message);
            }
        }


        private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response, string defaultMessage)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return $"{defaultMessage} (Status: {(int)response.StatusCode})";
                }

                using var doc = System.Text.Json.JsonDocument.Parse(content);
                var root = doc.RootElement;


                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    var errorList = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                var errStr = item.GetString();
                                if (!string.IsNullOrWhiteSpace(errStr))
                                {
                                    errorList.Add(errStr);
                                }
                            }
                        }
                        else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            var errStr = prop.Value.GetString();
                            if (!string.IsNullOrWhiteSpace(errStr))
                            {
                                errorList.Add(errStr);
                            }
                        }
                    }

                    if (errorList.Count > 0)
                    {
                        return string.Join("\n", errorList);
                    }
                }


                if (root.TryGetProperty("message", out var messageProp) && !string.IsNullOrWhiteSpace(messageProp.GetString()))
                {
                    return messageProp.GetString()!;
                }


                if (root.TryGetProperty("error", out var errorProp) && !string.IsNullOrWhiteSpace(errorProp.GetString()))
                {
                    return errorProp.GetString()!;
                }
            }
            catch
            {

            }

            return $"{defaultMessage} (Status: {(int)response.StatusCode})";
        }
    }
}