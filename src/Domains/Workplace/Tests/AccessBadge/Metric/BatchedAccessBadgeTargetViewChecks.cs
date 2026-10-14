namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedAccessBadgeTargetViewChecks
{
    public static void BatchedAccessBadgeTargetViewShowsTheStatusInCapitals()
    {
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("code-10", "title-996");
        var view = BatchedAccessBadgeTargetView.ToBatchedAccessBadgeTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAccessBadgeTargetView.ToBatchedAccessBadgeTargetViews(new List<BatchedAccessBadgeTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeTargetViewShowsTheStatusInCapitals();
    }
}
