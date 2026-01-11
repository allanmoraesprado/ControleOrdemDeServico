using OsService.Web.Api.Customers;
using OsService.Web.Api.ServiceOrders;

namespace OsService.Web.Api;

public sealed class OsServiceApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    public sealed record OpenServiceOrderResult(Guid Id, int Number);

    public async Task<Guid> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/v1/customers", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateCustomerResult>(cancellationToken: ct);
        return result!.Id;
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/v1/customers/{id}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: ct);
    }

    public async Task<CustomerResponse?> SearchCustomerAsync(string? phone, string? document, CancellationToken ct = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(phone))
            qs.Add($"phone={Uri.EscapeDataString(phone)}");
        if (!string.IsNullOrWhiteSpace(document))
            qs.Add($"document={Uri.EscapeDataString(document)}");

        var query = qs.Count > 0 ? "?" + string.Join("&", qs) : string.Empty;

        using var response = await _httpClient.GetAsync($"/v1/customers/search{query}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: ct);
    }

    public async Task<OpenServiceOrderResult> OpenServiceOrderAsync(OpenServiceOrderRequest request, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/v1/service-orders", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenServiceOrderResult>(cancellationToken: ct);
        return result!;
    }

    public async Task<ServiceOrderResponse?> GetServiceOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/v1/service-orders/{id}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(cancellationToken: ct);
    }
}