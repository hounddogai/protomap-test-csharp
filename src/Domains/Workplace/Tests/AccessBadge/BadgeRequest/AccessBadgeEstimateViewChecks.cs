namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class AccessBadgeEstimateViewChecks
{
    public static void AccessBadgeEstimateViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeEstimate.NewAccessBadgeEstimate("code-224", "title-870");
        var view = AccessBadgeEstimateView.ToAccessBadgeEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeEstimateView.ToAccessBadgeEstimateViews(new List<AccessBadgeEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeEstimateViewShowsTheStatusInCapitals();
    }
}
