
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BerthPlanner.Model;
using System.ComponentModel.DataAnnotations;

namespace BerthPlanner.Pages;

public class VesselRegistrationModel : PageModel
{
    [BindProperty]
    public int EditVesselId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "IMO Number is required.")]
    [RegularExpression(@"^\d{7}$",
        ErrorMessage = "IMO Number must contain exactly 7 digits.")]
    public string IMONumber { get; set; } = "";


    [BindProperty]
    [Required(ErrorMessage = "Vessel Name is required.")]
    [StringLength(50, MinimumLength = 3,
        ErrorMessage = "Vessel name must be between 3 and 50 characters.")]
    public string VesselName { get; set; } = "";

    [BindProperty]
    [Range(10, 500,
        ErrorMessage = "Length must be between 10 and 500 meters.")]
    public double Length { get; set; }

    [BindProperty]
    [Range(1, 30,
        ErrorMessage = "Draft must be between 1 and 30 meters.")]
    public double Draft { get; set; }

    [BindProperty]
    public CargoType CargoType { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Agent Name is required.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Agent name must be between 3 and 100 characters.")]
    public string AgentName { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Agent Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string AgentEmail { get; set; } = "";

    public List<Vessel> Vessels => PortData.Vessels;

    public void OnGet()
    {
    }

    public IActionResult OnPostAdd()
    {
        if (!ModelState.IsValid)
            return Page();

        if (!Enum.IsDefined(typeof(CargoType), CargoType))
        {
            ModelState.AddModelError(
                nameof(CargoType), "Invalid cargo type.");
            return Page();
        }

        bool exists = PortData.Vessels.Any(v =>
            v.IMONumber == IMONumber.Trim());

        if (exists)
        {
            ModelState.AddModelError(
                nameof(IMONumber),
                "A vessel with this IMO Number already exists.");

            return Page();
        }

        try
        {
            var vessel = new Vessel(
                IMONumber.Trim(),
                VesselName.Trim(),
                Length,
                Draft,
                CargoType,
                AgentName.Trim(),
                AgentEmail.Trim());

            PortData.Vessels.Add(vessel);

            TempData["Success"] =
                "Vessel registered successfully!";

            return RedirectToPage();
        }
        catch (Exception)
        {
            ModelState.AddModelError(
                "", "Unable to register the vessel.");
            return Page();
        }
    }

    public IActionResult OnPostEdit(
        int vesselId,
        string imoNumber,
        string vesselName,
        double length,
        double draft,
        CargoType cargoType,
        string agentName,
        string agentEmail)
    {
        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == vesselId);

        if (vessel == null)
        {
            TempData["Error"] = "Vessel not found.";
            return RedirectToPage();
        }

        imoNumber = imoNumber?.Trim() ?? "";
        vesselName = vesselName?.Trim() ?? "";
        agentName = agentName?.Trim() ?? "";
        agentEmail = agentEmail?.Trim() ?? "";

        if (!System.Text.RegularExpressions.Regex.IsMatch(
                imoNumber, @"^\d{7}$") ||
            string.IsNullOrWhiteSpace(vesselName) ||
            vesselName.Length < 3 ||
            vesselName.Length > 50 ||
            double.IsNaN(length) ||
            double.IsInfinity(length) ||
            length < 10 ||
            length > 500 ||
            double.IsNaN(draft) ||
            double.IsInfinity(draft) ||
            draft < 1 ||
            draft > 30 ||
            string.IsNullOrWhiteSpace(agentName) ||
            agentName.Length < 3 ||
            agentName.Length > 100 ||
            string.IsNullOrWhiteSpace(agentEmail) ||
            agentEmail.Length > 100 ||
            !new EmailAddressAttribute().IsValid(agentEmail) ||
            !Enum.IsDefined(typeof(CargoType), cargoType))
        {
            TempData["Error"] =
                "Invalid vessel information. Please check all fields.";

            return RedirectToPage();
        }

        bool duplicateIMO = PortData.Vessels.Any(v =>
            v.VesselId != vesselId &&
            v.IMONumber == imoNumber);

        if (duplicateIMO)
        {
            TempData["Error"] =
                "Another vessel already uses this IMO Number.";

            return RedirectToPage();
        }

        vessel.IMONumber = imoNumber;
        vessel.VesselName = vesselName;
        vessel.Length = length;
        vessel.Draft = draft;
        vessel.CargoType = cargoType;
        vessel.AgentName = agentName;
        vessel.AgentEmail = agentEmail;

        TempData["Success"] =
            "Vessel information updated successfully!";

        return RedirectToPage();
    }
}
