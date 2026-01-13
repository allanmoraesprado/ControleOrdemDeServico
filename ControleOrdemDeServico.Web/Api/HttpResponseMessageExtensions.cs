namespace OsService.Web.Api
{    
    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessWithApiErrorAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            ApiError? apiError = null;

            try
            {
                apiError = await response.Content.ReadFromJsonAsync<ApiError>();
            }
            catch
            {
            }

            var message = !string.IsNullOrWhiteSpace(apiError?.Error)
                ? apiError!.Error!
                : $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";

            throw new ApiException(message, response.StatusCode);
        }
    }
}
