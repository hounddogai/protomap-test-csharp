namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class FallbackAccessBadgeEstimateViewChecks
{
    public static void FallbackAccessBadgeEstimateViewShowsTheStatusInCapitals()
    {
        var item = FallbackAccessBadgeEstimate.NewFallbackAccessBadgeEstimate("code-199", "label-265");
        var view = FallbackAccessBadgeEstimateView.ToFallbackAccessBadgeEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAccessBadgeEstimateView.ToFallbackAccessBadgeEstimateViews(new List<FallbackAccessBadgeEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeEstimateViewShowsTheStatusInCapitals();
    }
}
