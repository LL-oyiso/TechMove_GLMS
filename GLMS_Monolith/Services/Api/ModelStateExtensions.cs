using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GLMS_Monolith.Services.Api;

public static class ModelStateExtensions
{
    // Surfaces API-side validation errors (from a 400 response) onto the MVC form.
    public static void AddApiErrors(this ModelStateDictionary modelState, ApiValidationException ex)
    {
        foreach (var (field, messages) in ex.Errors)
        {
            foreach (var message in messages)
            {
                modelState.AddModelError(field, message);
            }
        }
    }
}
