namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeAdjustmentViewChecks
{
    public static void AccessBadgeAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("code-77", "title-805");
        var view = AccessBadgeAdjustmentView.ToAccessBadgeAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeAdjustmentView.ToAccessBadgeAdjustmentViews(new List<AccessBadgeAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeAdjustmentViewShowsTheStatusInCapitals();
    }
}
