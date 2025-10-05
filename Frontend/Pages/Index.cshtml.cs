using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace DashboardWeb.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public List<DashboardItem>? DashboardItems { get; set; }
    public string? ErrorMessage { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task OnGetAsync()
    {
        await LoadDashboardData();
    }

    public async Task OnPostAsync()
    {
        await LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        try
        {
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync($"{apiBaseUrl}/api/dashboard");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                DashboardItems = JsonConvert.DeserializeObject<List<DashboardItem>>(content);
            }
            else
            {
                ErrorMessage = $"API returned status code: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to connect to API: {ex.Message}";
        }
    }
}

public class DashboardItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
