
using Microsoft.AspNetCore.Mvc.RazorPages;
using BerthPlanner.Model;

namespace BerthPlanner.Pages;

public class MyRequestsModel : PageModel
{
    // Get all submitted requests
    public List<BerthRequest> Requests =>
        PortData.Requests
            .OrderByDescending(r => r.RequestDate)
            .ThenByDescending(r => r.RequestId)
            .ToList();

    // Request statistics
    public int TotalRequests =>
        PortData.Requests.Count;

    public int PendingRequests =>
        PortData.Requests.Count(r =>
            r.Status == RequestStatus.Pending);

    public int ApprovedRequests =>
        PortData.Requests.Count(r =>
            r.Status == RequestStatus.Approved);

    public int RejectedRequests =>
        PortData.Requests.Count(r =>
            r.Status.ToString() == "Rejected");

    // Find vessel name
    public string GetVesselName(int vesselId)
    {
        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == vesselId);

        return vessel?.VesselName ?? "Unknown Vessel";
    }

    // Find vessel IMO number
    public string GetVesselIMO(int vesselId)
    {
        var vessel = PortData.Vessels
            .FirstOrDefault(v => v.VesselId == vesselId);

        return vessel?.IMONumber ?? "N/A";
    }

    // Find berth name
    public string GetBerthName(int berthId)
    {
        var berth = PortData.Berths
            .FirstOrDefault(b => b.BerthId == berthId);

        return berth?.BerthName ?? "Unknown Berth";
    }

    public void OnGet()
    {
    }
}
