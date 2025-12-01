using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Naviguard.Infrastructure.ExternalServices
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private string _jwtToken = string.Empty;

        public ApiClient(string baseAddress)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public async Task<(bool Success, long UserId, string Username, string Token)> LoginAsync(
            string username,
            string password)
        {
            var loginData = new { Username = username, Password = password };
            var jsonContent = JsonSerializer.Serialize(loginData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/Auth/login", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("JSON Recibido de la API:");
                    Debug.WriteLine(responseBody);

                    using var jsonDoc = JsonDocument.Parse(responseBody);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("token", out var tokenElement) &&
                        root.TryGetProperty("userId", out var userIdElement) &&
                        root.TryGetProperty("username", out var usernameElement) &&
                        tokenElement.GetString() is not null)
                    {
                        _jwtToken = tokenElement.GetString()!;
                        long apiUserId = userIdElement.GetInt64();
                        string user = usernameElement.GetString()!;

                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", _jwtToken);

                        return (true, apiUserId, user, _jwtToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error de conexión con la API: {ex.Message}");
            }

            return (false, 0, string.Empty, string.Empty);
        }

        public void Logout()
        {
            _jwtToken = string.Empty;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}