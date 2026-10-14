namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateOverrideViewChecks
{
    public static void RateOverrideViewShowsTheStatusInCapitals()
    {
        var item = RateOverride.NewRateOverride("code-381", "name-424");
        var view = RateOverrideView.ToRateOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateOverrideView.ToRateOverrideViews(new List<RateOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateOverrideViewShowsTheStatusInCapitals();
    }
}
