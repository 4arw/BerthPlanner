
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace BerthPlanner.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId =>
        !string.IsNullOrEmpty(RequestId);

    public int? ErrorStatusCode { get; set; }

    public void OnGet(int? statusCode)
    {
        ErrorStatusCode = statusCode;

        RequestId = Activity.Current?.Id
            ?? HttpContext.TraceIdentifier;
    }
}
