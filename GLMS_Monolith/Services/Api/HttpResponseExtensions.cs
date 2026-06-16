using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace GLMS_Monolith.Services.Api;

public static class HttpResponseExtensions
{
    // Turns a non-success API response into a typed exception the controllers can handle:
    // 400 -> ApiValidationException (field errors surfaced on the form), everything else -> ApiException.
    public static async Task EnsureApiSuccessAsync(this HttpResponseMessage response, CancellationToken ct = default)
    {
        if (response.IsSuccessStatusCode) return;

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: ct);
                if (problem?.Errors is { Count: > 0 })
                {
                    throw new ApiValidationException(problem.Errors);
                }
            }
            catch (ApiValidationException)
            {
                throw;
            }
            catch
            {
                // Fall through to the generic exception below.
            }
        }

        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "You are not signed in or your session has expired.",
            HttpStatusCode.NotFound => "The requested item was not found.",
            HttpStatusCode.ServiceUnavailable => "A dependent service is currently unavailable. Please try again.",
            _ => $"The API request failed ({(int)response.StatusCode} {response.ReasonPhrase})."
        };

        throw new ApiException((int)response.StatusCode, message);
    }
}
