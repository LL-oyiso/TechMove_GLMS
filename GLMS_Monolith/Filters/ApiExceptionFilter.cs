using GLMS_Monolith.Models;
using GLMS_Monolith.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GLMS_Monolith.Filters;

// Centralised handling for failed API calls. Validation errors (400) are left to controllers
// so they can re-render the form; everything else renders a friendly error page.
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiValidationException)
        {
            return; // handled inline by the controller action
        }

        if (context.Exception is ApiException apiException)
        {
            _logger.LogWarning(apiException, "API call failed with status {StatusCode}", apiException.StatusCode);

            // 401 -> send the user to the login page to authenticate.
            if (apiException.StatusCode == StatusCodes.Status401Unauthorized)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                context.ExceptionHandled = true;
                return;
            }

            var model = new ApiErrorViewModel
            {
                StatusCode = apiException.StatusCode,
                Message = apiException.Message
            };

            context.Result = new ViewResult
            {
                ViewName = "ApiError",
                ViewData = new ViewDataDictionary<ApiErrorViewModel>(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
                {
                    Model = model
                }
            };
            context.ExceptionHandled = true;
        }
    }
}
