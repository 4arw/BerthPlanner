
using Microsoft.AspNetCore.Mvc.RazorPages;
using BerthPlanner.Model;

namespace BerthPlanner.Pages;

public class IndexModel : PageModel
{
    public int TotalVessels =>
        PortData.Vessels.Count;

    public int TotalBerths =>
        PortData.Berths.Count;

    public int AvailableBerths =>
        PortData.Berths.Count(b =>
            b.Status == BerthStatus.Available);

    public int TotalRequests =>
        PortData.Requests.Count;

    public int PendingRequests =>
        PortData.Requests.Count(r =>
            r.Status == RequestStatus.Pending);

    public int ApprovedRequests =>
        PortData.Requests.Count(r =>
            r.Status == RequestStatus.Approved);

    public List<BerthRequest> RecentRequests =>
        PortData.Requests
            .OrderByDescending(r => r.RequestDate)
            .ThenByDescending(r => r.RequestId)
            .Take(5)
            .ToList();

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

    public void OnGet()
    {
    }
}
