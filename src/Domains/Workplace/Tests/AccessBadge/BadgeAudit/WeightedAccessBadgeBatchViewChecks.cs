namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class WeightedAccessBadgeBatchViewChecks
{
    public static void WeightedAccessBadgeBatchViewShowsTheStatusInCapitals()
    {
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("code-430", "label-124");
        var view = WeightedAccessBadgeBatchView.ToWeightedAccessBadgeBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedAccessBadgeBatchView.ToWeightedAccessBadgeBatchViews(new List<WeightedAccessBadgeBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeBatchViewShowsTheStatusInCapitals();
    }
}
