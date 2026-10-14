namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentItemViewChecks
{
    public static void AdjustmentItemViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentItem.NewAdjustmentItem("code-544", "title-41");
        var view = AdjustmentItemView.ToAdjustmentItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentItemView.ToAdjustmentItemViews(new List<AdjustmentItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentItemViewShowsTheStatusInCapitals();
    }
}
