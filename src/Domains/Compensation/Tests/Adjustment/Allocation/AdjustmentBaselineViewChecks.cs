namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentBaselineViewChecks
{
    public static void AdjustmentBaselineViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentBaseline.NewAdjustmentBaseline("code-910", "title-989");
        var view = AdjustmentBaselineView.ToAdjustmentBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentBaselineView.ToAdjustmentBaselineViews(new List<AdjustmentBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentBaselineViewShowsTheStatusInCapitals();
    }
}
