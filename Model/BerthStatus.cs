using System.ComponentModel.DataAnnotations;

namespace BerthPlanner.Model;

public enum BerthStatus
{
    Available,
    Inactive,
    [Display(Name ="Currently Under Maintenance!")] Maintenance,
}