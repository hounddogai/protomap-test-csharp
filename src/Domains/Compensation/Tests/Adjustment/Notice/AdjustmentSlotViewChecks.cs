namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSlotViewChecks
{
    public static void AdjustmentSlotViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentSlot.NewAdjustmentSlot("code-783", "title-756");
        var view = AdjustmentSlotView.ToAdjustmentSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentSlotView.ToAdjustmentSlotViews(new List<AdjustmentSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentSlotViewShowsTheStatusInCapitals();
    }
}
