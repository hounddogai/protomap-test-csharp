namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AccessBadgeAllocationViewChecks
{
    public static void AccessBadgeAllocationViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("code-762", "label-772");
        var view = AccessBadgeAllocationView.ToAccessBadgeAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeAllocationView.ToAccessBadgeAllocationViews(new List<AccessBadgeAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeAllocationViewShowsTheStatusInCapitals();
    }
}
