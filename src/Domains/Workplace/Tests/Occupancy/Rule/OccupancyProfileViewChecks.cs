namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class OccupancyProfileViewChecks
{
    public static void OccupancyProfileViewShowsTheStatusInCapitals()
    {
        var item = OccupancyProfile.NewOccupancyProfile("code-289", "title-856");
        var view = OccupancyProfileView.ToOccupancyProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyProfileView.ToOccupancyProfileViews(new List<OccupancyProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyProfileViewShowsTheStatusInCapitals();
    }
}
