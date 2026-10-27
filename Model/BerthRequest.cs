namespace BerthPlanner.Model;

public class BerthRequest
{
                                        //static values used for logic
    private static int nextId = 1;
    public static readonly TimeSpan TurnaroundGap = TimeSpan.FromHours(2);
    
                                        //attributes
    public int RequestId {get;set;}
    public int VesselId {get; set;}
    public int BerthId {get; set;}
    public DateTime ArrivalTime {get; set; }
    public DateTime DepartureTime {get; set; }
    public DateTime RequestDate {get; set; } = DateTime.Now;
    public RequestStatus Status {get; set; } = RequestStatus.Pending;
    public string? SupervisorNote { get; set;}
    
                                        //constructors
    public BerthRequest(){}

    public BerthRequest(int vesselId, int berthId, DateTime arrivalTime, DateTime departureTime, DateTime requestDate)
    {
        RequestId = nextId++;
        VesselId = vesselId;
        BerthId = berthId;
        ArrivalTime = arrivalTime;
        DepartureTime = departureTime;
    }
    
                                        //methods
    public void AssignId()
    {
        RequestId = nextId++;
    }

    public TimeSpan duration()
    {
        return ArrivalTime - DepartureTime;
    }

    public bool conflictsWith(BerthRequest otherRequest)
    {
        return(BerthId == otherRequest.BerthId && RequestId != otherRequest.RequestId
                                        && ArrivalTime < otherRequest.DepartureTime + TurnaroundGap &&
            otherRequest.ArrivalTime < DepartureTime + TurnaroundGap);
    }

    public void approve(string? note = null)
    {
        Status = RequestStatus.Approved;
        SupervisorNote = note;
    }
}