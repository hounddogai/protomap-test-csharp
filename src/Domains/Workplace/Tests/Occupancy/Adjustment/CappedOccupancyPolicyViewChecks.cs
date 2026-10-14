namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedOccupancyPolicyViewChecks
{
    public static void CappedOccupancyPolicyViewShowsTheStatusInCapitals()
    {
        var item = CappedOccupancyPolicy.NewCappedOccupancyPolicy("code-696", "name-924");
        var view = CappedOccupancyPolicyView.ToCappedOccupancyPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedOccupancyPolicyView.ToCappedOccupancyPolicyViews(new List<CappedOccupancyPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedOccupancyPolicyViewShowsTheStatusInCapitals();
    }
}
