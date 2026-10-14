namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class LegacyOccupancyExceptionViewChecks
{
    public static void LegacyOccupancyExceptionViewShowsTheStatusInCapitals()
    {
        var item = LegacyOccupancyException.NewLegacyOccupancyException("code-784", "title-249");
        var view = LegacyOccupancyExceptionView.ToLegacyOccupancyExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyOccupancyExceptionView.ToLegacyOccupancyExceptionViews(new List<LegacyOccupancyException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyOccupancyExceptionViewShowsTheStatusInCapitals();
    }
}
