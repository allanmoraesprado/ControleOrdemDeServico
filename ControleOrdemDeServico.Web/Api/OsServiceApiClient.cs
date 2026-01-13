using OsService.Web.Api.Customers;
using OsService.Web.Api.ServiceOrders;
using System.Net.Http.Headers;

namespace OsService.Web.Api;

public sealed class OsServiceApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    public Uri? BaseAddress => _httpClient.BaseAddress;

    public sealed record OpenServiceOrderResult(Guid Id, int Number);

    private static ApiException EmptyResponseException(HttpResponseMessage response, string operation) =>
        new($"API returned no data when {operation}.", response.StatusCode);

    public async Task<Guid> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/v1/customers", request, ct);
        await response.EnsureSuccessWithApiErrorAsync();

        var result = await response.Content.ReadFromJsonAsync<CreateCustomerResult>(cancellationToken: ct)
            ?? throw EmptyResponseException(response, "creating the customer");

        return result!.Id;
    }

    public async Task<CustomerResponse?> GetCustomerByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/v1/customers/{id}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await response.EnsureSuccessWithApiErrorAsync();

        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: ct)
            ?? throw EmptyResponseException(response, "fetching the customer by Id");

        return result;
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

        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: ct)
            ?? throw EmptyResponseException(response, "searching for a customer");

        return result;
    }

    public async Task<OpenServiceOrderResult> OpenServiceOrderAsync(OpenServiceOrderRequest request, CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/v1/service-orders", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenServiceOrderResult>(cancellationToken: ct)
            ?? throw EmptyResponseException(response, "opening the service order");

        return result;
    }

    public async Task<ServiceOrderResponse?> GetServiceOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/v1/service-orders/{id}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await response.EnsureSuccessWithApiErrorAsync();

        var result = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(cancellationToken: ct)
            ?? throw EmptyResponseException(response, "fetching the service order by Id");

        return result;
    }

    public async Task<ServiceOrderResponse> ChangeServiceOrderStatusAsync(
    Guid id,
    ChangeServiceOrderStatusRequest request,
    CancellationToken ct = default)
    {
        using var response = await PatchAsJsonAsync($"/v1/service-orders/{id}/status", request, ct);
        await response.EnsureSuccessWithApiErrorAsync();

        var result = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(cancellationToken: ct)
            ?? throw EmptyResponseException(response, "changing the service order status");

        return result;
    }

    public async Task<ServiceOrderResponse> UpdateServiceOrderPriceAsync(
        Guid id,
        UpdateServiceOrderPriceRequest request,
        CancellationToken ct = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"/v1/service-orders/{id}/price", request, ct);
        await response.EnsureSuccessWithApiErrorAsync();

        var result = await response.Content.ReadFromJsonAsync<ServiceOrderResponse>(cancellationToken: ct)
            ?? throw EmptyResponseException(response, "updating the service order price");

        return result;
    }

    public async Task UploadBeforeAttachmentAsync(
        Guid id,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        using var response = await _httpClient.PostAsync($"/v1/service-orders/{id}/attachments/before", form, ct);
        await response.EnsureSuccessWithApiErrorAsync();
    }

    public async Task UploadAfterAttachmentAsync(
        Guid id,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        using var response = await _httpClient.PostAsync($"/v1/service-orders/{id}/attachments/after", form, ct);
        await response.EnsureSuccessWithApiErrorAsync();
    }

    public async Task<IReadOnlyList<ServiceOrderAttachmentResponse>> GetServiceOrderAttachmentsAsync(
        Guid id,
        CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync($"/v1/service-orders/{id}/attachments", ct);
        await response.EnsureSuccessWithApiErrorAsync();

        var result = await response.Content.ReadFromJsonAsync<List<ServiceOrderAttachmentResponse>>(cancellationToken: ct)
            ?? new List<ServiceOrderAttachmentResponse>();

        return result;
    }

    private Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        string url,
        T body,
        CancellationToken ct = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(body)
        };

        return _httpClient.SendAsync(message, ct);
    }
}