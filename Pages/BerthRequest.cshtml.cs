
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BerthPlanner.Model;

namespace BerthPlanner.Pages;

public class BerthRequestModel : PageModel
{
    [BindProperty]
    public int VesselId { get; set; }

    [BindProperty]
    public int BerthId { get; set; }

    [BindProperty]
    public DateTime ArrivalTime { get; set; }

    [BindProperty]
    public DateTime DepartureTime { get; set; }

    public List<Vessel> Vessels => PortData.Vessels;

    public List<Berth> Berths => PortData.Berths;

    public List<BerthRequest> Requests =>
        PortData.Requests
            .OrderByDescending(r => r.RequestDate)
            .ToList();

    public string? ErrorMessage { get; set; }

    public DateTime? SuggestedArrival { get; set; }

    public DateTime? SuggestedDeparture { get; set; }

    public List<Berth> AlternativeBerths { get; set; } = new();

    public void OnGet()
    {
        ArrivalTime = DateTime.Now.AddDays(1);
        ArrivalTime = new DateTime(
            ArrivalTime.Year,
            ArrivalTime.Month,
            ArrivalTime.Day,
            10, 0, 0);

        DepartureTime = ArrivalTime.AddHours(6);
    }

    public IActionResult OnPost()
    {
        if (VesselId <= 0 || BerthId <= 0)
        {
            ErrorMessage = "Please select a vessel and berth.";
            return Page();
        }

        if (ArrivalTime <= DateTime.Now)
        {
            ErrorMessage = "Arrival time must be in the future.";
            return Page();
        }

        if (DepartureTime <= ArrivalTime)
        {
            ErrorMessage =
                "Departure time must be after arrival time.";
            return Page();
        }

        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == VesselId);

        var berth = PortData.Berths
            .FirstOrDefault(b => b.BerthId == BerthId);

        if (vessel == null)
        {
            ErrorMessage = "Selected vessel was not found.";
            return Page();
        }

        if (berth == null)
        {
            ErrorMessage = "Selected berth was not found.";
            return Page();
        }

        if (!berth.isAvailable())
        {
            ErrorMessage = "The selected berth is not available.";
            FindAlternativeBerths(vessel);
            return Page();
        }

        if (!berth.canFit(vessel))
        {
            ErrorMessage =
                "The vessel does not fit the selected berth.";

            FindAlternativeBerths(vessel);
            return Page();
        }

        if (HasConflict(BerthId, ArrivalTime, DepartureTime))
        {
            ErrorMessage =
                "The selected berth has a scheduling conflict.";

            SuggestedArrival = FindNextAvailableTime(
                BerthId, ArrivalTime, DepartureTime);

            if (SuggestedArrival.HasValue)
            {
                SuggestedDeparture =
                    SuggestedArrival.Value +
                    (DepartureTime - ArrivalTime);
            }

            FindAlternativeBerths(vessel);
            return Page();
        }

        var request = new BerthRequest(
            VesselId,
            BerthId,
            ArrivalTime,
            DepartureTime,
            DateTime.Now);

        request.RequestDate = DateTime.Now;
        request.Status = RequestStatus.Pending;

        PortData.Requests.Add(request);

        TempData["Success"] =
            "Berth request submitted successfully!";

        return RedirectToPage();
    }


    // ================= AJAX HANDLERS (used by jQuery) =================

    // AJAX: details of the selected vessel
    public JsonResult OnGetVesselInfo(int id)
    {
        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == id);

        if (vessel == null)
            return new JsonResult(new { found = false });

        string priority = vessel.CargoType switch
        {
            CargoType.Medical => "High",
            CargoType.Food => "High",
            CargoType.Fuel => "Medium",
            _ => "Normal"
        };

        return new JsonResult(new
        {
            found = true,
            imo = vessel.IMONumber,
            cargo = vessel.CargoType.ToString(),
            priority,
            agent = vessel.AgentName
        });
    }

    // AJAX: check the request before submitting
    public JsonResult OnGetCheck(
        int vesselId,
        int berthId,
        DateTime arrival,
        DateTime departure)
    {
        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == vesselId);

        var berth = PortData.Berths
            .FirstOrDefault(b => b.BerthId == berthId);

        if (vessel == null || berth == null)
            return CheckResult(false, "Please select a vessel and a berth.");

        if (arrival <= DateTime.Now)
            return CheckResult(false, "Arrival time must be in the future.");

        if (departure <= arrival)
            return CheckResult(false, "Departure time must be after arrival time.");

        if (!berth.isAvailable())
            return CheckResult(false,
                $"{berth.BerthName} is not available ({berth.Status}).");

        if (!berth.canFit(vessel))
            return CheckResult(false,
                $"{vessel.VesselName} does not fit {berth.BerthName} (length or draft is too large).");

        if (HasConflict(berthId, arrival, departure))
        {
            DateTime? next = FindNextAvailableTime(berthId, arrival, departure);

            if (next.HasValue)
            {
                DateTime nextDeparture = next.Value + (departure - arrival);
                return CheckResult(false,
                    $"{berth.BerthName} is occupied during this period.",
                    next.Value, nextDeparture);
            }

            return CheckResult(false,
                $"{berth.BerthName} is occupied during this period.");
        }

        return CheckResult(true,
            $"{berth.BerthName} is available for {vessel.VesselName}.");
    }

    private static JsonResult CheckResult(
        bool ok,
        string message,
        DateTime? suggestedArrival = null,
        DateTime? suggestedDeparture = null)
    {
        return new JsonResult(new
        {
            ok,
            message,
            suggestedArrival = suggestedArrival?.ToString("yyyy-MM-ddTHH:mm"),
            suggestedDeparture = suggestedDeparture?.ToString("yyyy-MM-ddTHH:mm"),
            suggestionText = suggestedArrival.HasValue && suggestedDeparture.HasValue
                ? $"{suggestedArrival:dd MMM yyyy HH:mm} to {suggestedDeparture:dd MMM yyyy HH:mm}"
                : null
        });
    }


    private bool HasConflict(
        int berthId,
        DateTime arrival,
        DateTime departure)
    {
        return PortData.Requests.Any(r =>
            r.BerthId == berthId &&
            r.Status == RequestStatus.Approved &&
            arrival <
                r.DepartureTime + BerthRequest.TurnaroundGap &&
            r.ArrivalTime <
                departure + BerthRequest.TurnaroundGap);
    }

    public DateTime? FindNextAvailableTime(
        int berthId,
        DateTime arrival,
        DateTime departure)
    {
        if (departure <= arrival)
            return null;

        TimeSpan duration = departure - arrival;
        DateTime candidate = arrival;

        var bookings = PortData.Requests
            .Where(r =>
                r.BerthId == berthId &&
                r.Status == RequestStatus.Approved)
            .OrderBy(r => r.ArrivalTime)
            .ToList();

        foreach (var booking in bookings)
        {
            bool conflict =
                candidate <
                    booking.DepartureTime + BerthRequest.TurnaroundGap
                &&
                booking.ArrivalTime <
                    candidate + duration + BerthRequest.TurnaroundGap;

            if (conflict)
            {
                candidate =
                    booking.DepartureTime +
                    BerthRequest.TurnaroundGap;
            }
        }

        return candidate > arrival ? candidate : null;
    }

    private void FindAlternativeBerths(Vessel vessel)
    {
        AlternativeBerths = PortData.Berths
            .Where(b =>
                b.BerthId != BerthId &&
                b.isAvailable() &&
                b.canFit(vessel) &&
                !HasConflict(
                    b.BerthId,
                    ArrivalTime,
                    DepartureTime))
            .ToList();
    }

    public string GetVesselName(int vesselId)
    {
        return PortData.Vessels
            .FirstOrDefault(v => v.VesselId == vesselId)?
            .VesselName ?? "Unknown Vessel";
    }

    public string GetBerthName(int berthId)
    {
        return PortData.Berths
            .FirstOrDefault(b => b.BerthId == berthId)?
            .BerthName ?? "Unknown Berth";
    }
}
