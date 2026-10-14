namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class RateBaselineViewChecks
{
    public static void RateBaselineViewShowsTheStatusInCapitals()
    {
        var item = RateBaseline.NewRateBaseline("code-410", "name-822");
        var view = RateBaselineView.ToRateBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateBaselineView.ToRateBaselineViews(new List<RateBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateBaselineViewShowsTheStatusInCapitals();
    }
}
