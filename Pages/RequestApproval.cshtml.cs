
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BerthPlanner.Model;

namespace BerthPlanner.Pages;

public class RequestApprovalModel : PageModel
{
    public List<BerthRequest> Requests =>
        PortData.Requests
            .OrderBy(r => r.Status != RequestStatus.Pending)
            .ThenBy(r => GetPriorityNumber(r.VesselId))
            .ThenBy(r => r.ArrivalTime)
            .ToList();

    public int PendingCount =>
        PortData.Requests.Count(r =>
            r.Status == RequestStatus.Pending);

    public int ApprovedCount =>
        PortData.Requests.Count(r =>
            r.Status == RequestStatus.Approved);

    public int RejectedCount =>
        PortData.Requests.Count(r =>
            r.Status.ToString() == "Rejected");

    public void OnGet()
    {
    }

    public string GetVesselName(int id)
    {
        return PortData.Vessels
            .FirstOrDefault(v => v.VesselId == id)?
            .VesselName ?? "Unknown Vessel";
    }

    public string GetBerthName(int id)
    {
        return PortData.Berths
            .FirstOrDefault(b => b.BerthId == id)?
            .BerthName ?? "Unknown Berth";
    }

    public string GetCargoType(int vesselId)
    {
        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == vesselId);

        return vessel?.CargoType.ToString() ?? "Unknown";
    }

    public int GetPriorityNumber(int vesselId)
    {
        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == vesselId);

        if (vessel == null)
            return 3;

        return vessel.CargoType switch
        {
            CargoType.Medical => 1,
            CargoType.Food => 1,
            CargoType.Fuel => 2,
            _ => 3
        };
    }

    public string GetPriorityLabel(int vesselId)
    {
        return GetPriorityNumber(vesselId) switch
        {
            1 => "High",
            2 => "Medium",
            _ => "Normal"
        };
    }

    public IActionResult OnPostApprove(
        int requestId,
        string? supervisorNote)
    {
        try
        {
            var request = PortData.Requests
                .FirstOrDefault(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToPage();
            }

            if (request.Status != RequestStatus.Pending)
            {
                TempData["Error"] =
                    "Only pending requests can be approved.";
                return RedirectToPage();
            }

            var vessel = PortData.Vessels
                .FirstOrDefault(v =>
                    v.VesselId == request.VesselId);

            var berth = PortData.Berths
                .FirstOrDefault(b =>
                    b.BerthId == request.BerthId);

            if (vessel == null || berth == null)
            {
                TempData["Error"] =
                    "Vessel or berth not found.";
                return RedirectToPage();
            }

            if (request.ArrivalTime <= DateTime.Now ||
                request.DepartureTime <= request.ArrivalTime)
            {
                TempData["Error"] =
                    "Request dates are no longer valid.";
                return RedirectToPage();
            }

            if (!berth.isAvailable())
            {
                TempData["Error"] =
                    "The berth is currently unavailable.";
                return RedirectToPage();
            }

            if (!berth.canFit(vessel))
            {
                TempData["Error"] =
                    "The vessel does not fit this berth.";
                return RedirectToPage();
            }

            bool conflict = PortData.Requests.Any(r =>
                r.RequestId != request.RequestId &&
                r.BerthId == request.BerthId &&
                r.Status == RequestStatus.Approved &&
                request.ArrivalTime <
                    r.DepartureTime + BerthRequest.TurnaroundGap &&
                r.ArrivalTime <
                    request.DepartureTime + BerthRequest.TurnaroundGap);

            if (conflict)
            {
                TempData["Error"] =
                    "Cannot approve: scheduling conflict detected.";
                return RedirectToPage();
            }

            request.approve(supervisorNote?.Trim());

            TempData["Success"] =
                "Request approved successfully!";

            return RedirectToPage();
        }
        catch (Exception)
        {
            TempData["Error"] =
                "An error occurred while approving the request.";
            return RedirectToPage();
        }
    }

    public IActionResult OnPostReject(
        int requestId,
        string? supervisorNote)
    {
        try
        {
            var request = PortData.Requests
                .FirstOrDefault(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToPage();
            }

            if (request.Status != RequestStatus.Pending)
            {
                TempData["Error"] =
                    "Only pending requests can be rejected.";
                return RedirectToPage();
            }

            if (!Enum.TryParse<RequestStatus>(
                "Rejected", out var rejectedStatus))
            {
                TempData["Error"] =
                    "Rejected status is not defined.";
                return RedirectToPage();
            }

            request.Status = rejectedStatus;
            request.SupervisorNote =
                supervisorNote?.Trim();

            TempData["Success"] =
                "Request rejected successfully!";

            return RedirectToPage();
        }
        catch (Exception)
        {
            TempData["Error"] =
                "An error occurred while rejecting the request.";
            return RedirectToPage();
        }
    }
}
