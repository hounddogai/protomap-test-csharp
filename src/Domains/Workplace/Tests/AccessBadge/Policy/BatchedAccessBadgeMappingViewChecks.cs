namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BatchedAccessBadgeMappingViewChecks
{
    public static void BatchedAccessBadgeMappingViewShowsTheStatusInCapitals()
    {
        var item = BatchedAccessBadgeMapping.NewBatchedAccessBadgeMapping("code-977", "label-599");
        var view = BatchedAccessBadgeMappingView.ToBatchedAccessBadgeMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAccessBadgeMappingView.ToBatchedAccessBadgeMappingViews(new List<BatchedAccessBadgeMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeMappingViewShowsTheStatusInCapitals();
    }
}
