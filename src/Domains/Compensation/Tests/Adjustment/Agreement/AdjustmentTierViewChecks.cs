namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentTierViewChecks
{
    public static void AdjustmentTierViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentTier.NewAdjustmentTier("code-77", "title-971");
        var view = AdjustmentTierView.ToAdjustmentTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentTierView.ToAdjustmentTierViews(new List<AdjustmentTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentTierViewShowsTheStatusInCapitals();
    }
}
