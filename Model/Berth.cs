namespace BerthPlanner.Model;

public class Berth
{
    private static int nextId = 1; //to automate creating the next by incrementing
    public int BerthId { get; set; }
    public string BerthName { get; set; } = string.Empty;
    public double MaxLength { get; set; }
    public double MaxDepth { get; set; }
    public BerthStatus Status { get; set; } = BerthStatus.Available;

    public Berth()
    {
    }

    public Berth(string berthName, double maxLength, double maxDepth, BerthStatus status = BerthStatus.Available)
    {
        BerthId = nextId++;
        BerthName = berthName;
        MaxLength = maxLength;
        MaxDepth = maxDepth;
        Status = status;
    }
                                                        //methods
    public void AssignId()
    {
        BerthId = nextId++;
    }

    public bool isAvailable()
    {
        return Status == BerthStatus.Available;
    }
    
    public bool canFit(Vessel vessel)
    {
        return (vessel.Length <= MaxLength && vessel.Draft <= MaxDepth);
    }

    public override string ToString()
    {
        return $"{BerthName}";
    }
}