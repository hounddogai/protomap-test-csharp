namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalAdjustmentRollupViewChecks
{
    public static void GlobalAdjustmentRollupViewShowsTheStatusInCapitals()
    {
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("code-576", "name-767");
        var view = GlobalAdjustmentRollupView.ToGlobalAdjustmentRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAdjustmentRollupView.ToGlobalAdjustmentRollupViews(new List<GlobalAdjustmentRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAdjustmentRollupViewShowsTheStatusInCapitals();
    }
}
