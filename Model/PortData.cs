namespace BerthPlanner.Model;

public class PortData
{
    public static List<Vessel> Vessels { get; } = new()
    {
        new Vessel("2342345", "Al Sohar", 200, 10, CargoType.Container, "Ahmed Alkindi", "a.kendi@gmail.com"),
        new Vessel("6532345", "Sultana", 275.34, 23.6, CargoType.Medical, "Salim Saleh", "salimSS@gmail.com")
    };

    public static List<Berth> Berths { get; } = new()
    {
        new Berth("Berth A31", 265, 23),
        new Berth("Berth A32", 342, 23, BerthStatus.Maintenance),
        new Berth("Berth A33", 654, 12),
    };

    public static List<BerthRequest> Requests { get; } = new();
}