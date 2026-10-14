namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class GlobalAdjustmentBundleViewChecks
{
    public static void GlobalAdjustmentBundleViewShowsTheStatusInCapitals()
    {
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("code-732", "name-226");
        var view = GlobalAdjustmentBundleView.ToGlobalAdjustmentBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAdjustmentBundleView.ToGlobalAdjustmentBundleViews(new List<GlobalAdjustmentBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAdjustmentBundleViewShowsTheStatusInCapitals();
    }
}
