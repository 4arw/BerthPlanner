namespace BerthPlanner.Model;

public class Vessel
{
    private static int nextId = 1;          //id that will be used to generate ids
    
                                            //vessel attributes
    public int VesselId { get; set; }
    public string IMONumber { get; set; } = string.Empty;
    public string VesselName { get; set; } = string.Empty;
    public double Length { get; set; }
    public double Draft {get; set;}
    public CargoType CargoType { get; set; } = CargoType.General; //set the default type as general cargo
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    
                                            //constructor
    public Vessel(){}

    public Vessel(string imoNumber, string name, double length, double draft,
        CargoType cargo, string agentName, string email)
    {
        VesselId = nextId++;
        IMONumber = imoNumber;
        VesselName = name;
        Length = length;
        Draft = draft;
        CargoType = cargo;
        AgentName = agentName;
        AgentEmail = email;
    }

                                            //extra methods
    public void AssignId()
    { VesselId = nextId++; //assigning a vessel new id
    }

    public bool isPriorityCargo()
    {
        return (CargoType == CargoType.Medical || CargoType == CargoType.Food);
    }

    public override string ToString()
    {
        return $"{VesselName} (IMO {IMONumber})";
    }



}