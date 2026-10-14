namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateTierViewChecks
{
    public static void RateTierViewShowsTheStatusInCapitals()
    {
        var item = RateTier.NewRateTier("code-101", "name-832");
        var view = RateTierView.ToRateTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateTierView.ToRateTierViews(new List<RateTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateTierViewShowsTheStatusInCapitals();
    }
}
