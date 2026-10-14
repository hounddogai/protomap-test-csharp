namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedAccessBadgeThresholdViewChecks
{
    public static void CappedAccessBadgeThresholdViewShowsTheStatusInCapitals()
    {
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("code-931", "title-878");
        var view = CappedAccessBadgeThresholdView.ToCappedAccessBadgeThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedAccessBadgeThresholdView.ToCappedAccessBadgeThresholdViews(new List<CappedAccessBadgeThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedAccessBadgeThresholdViewShowsTheStatusInCapitals();
    }
}
