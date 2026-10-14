namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateMappingViewChecks
{
    public static void RateMappingViewShowsTheStatusInCapitals()
    {
        var item = RateMapping.NewRateMapping("code-173", "title-390");
        var view = RateMappingView.ToRateMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateMappingView.ToRateMappingViews(new List<RateMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateMappingViewShowsTheStatusInCapitals();
    }
}
