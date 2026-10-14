namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyGroupViewChecks
{
    public static void OccupancyGroupViewShowsTheStatusInCapitals()
    {
        var item = OccupancyGroup.NewOccupancyGroup("code-615", "name-128");
        var view = OccupancyGroupView.ToOccupancyGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyGroupView.ToOccupancyGroupViews(new List<OccupancyGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyGroupViewShowsTheStatusInCapitals();
    }
}
