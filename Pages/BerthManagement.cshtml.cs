
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BerthPlanner.Model;
using System.ComponentModel.DataAnnotations;

namespace BerthPlanner.Pages;

public class BerthManagementModel : PageModel
{
    [BindProperty]
    [Required]
    public string BerthName { get; set; } = "";

    [BindProperty]
    [Range(0.01, 10000)]
    public double MaxLength { get; set; }

    [BindProperty]
    [Range(0.01, 1000)]
    public double MaxDepth { get; set; }

    [BindProperty]
    public BerthStatus Status { get; set; } =
        BerthStatus.Available;

    public List<Berth> Berths => PortData.Berths;

    public void OnGet()
    {
    }

    // Add a new berth
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] =
                "Please enter valid berth information.";
            return Page();
        }

        try
        {
            string name = BerthName.Trim();

            if (PortData.Berths.Any(b =>
                b.BerthName.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                TempData["Error"] =
                    "This berth name already exists.";
                return Page();
            }

            Berth berth = new Berth(
                name,
                MaxLength,
                MaxDepth,
                Status
            );

            PortData.Berths.Add(berth);

            TempData["Success"] =
                "Berth added successfully!";

            return RedirectToPage();
        }
        catch (Exception)
        {
            TempData["Error"] =
                "An error occurred while adding the berth.";
            return Page();
        }
    }

    // Update berth status
    public IActionResult OnPostUpdateStatus(
        int berthId,
        BerthStatus newStatus)
    {
        try
        {
            if (!Enum.IsDefined(newStatus))
            {
                TempData["Error"] = "Invalid berth status.";
                return RedirectToPage();
            }

            var berth = PortData.Berths
                .FirstOrDefault(b => b.BerthId == berthId);

            if (berth == null)
            {
                TempData["Error"] = "Berth not found.";
                return RedirectToPage();
            }

            berth.Status = newStatus;

            TempData["Success"] =
                "Berth status updated successfully!";

            return RedirectToPage();
        }
        catch (Exception)
        {
            TempData["Error"] =
                "An error occurred while updating berth status.";
            return RedirectToPage();
        }
    }

    // Edit berth information
    public IActionResult OnPostEditBerth(
        int berthId,
        string newName,
        double newLength,
        double newDepth)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newName) ||
                newLength <= 0 || newLength > 10000 ||
                newDepth <= 0 || newDepth > 1000)
            {
                TempData["Error"] =
                    "Please enter valid berth information.";
                return RedirectToPage();
            }

            var berth = PortData.Berths
                .FirstOrDefault(b => b.BerthId == berthId);

            if (berth == null)
            {
                TempData["Error"] = "Berth not found.";
                return RedirectToPage();
            }

            string name = newName.Trim();

            if (PortData.Berths.Any(b =>
                b.BerthId != berthId &&
                b.BerthName.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                TempData["Error"] =
                    "This berth name already exists.";
                return RedirectToPage();
            }

            berth.BerthName = name;
            berth.MaxLength = newLength;
            berth.MaxDepth = newDepth;

            TempData["Success"] =
                "Berth updated successfully!";

            return RedirectToPage();
        }
        catch (Exception)
        {
            TempData["Error"] =
                "An error occurred while updating the berth.";
            return RedirectToPage();
        }
    }
}
