namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class TieredPayRunMilestoneViewChecks
{
    public static void TieredPayRunMilestoneViewShowsTheStatusInCapitals()
    {
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("code-648", "label-331");
        var view = TieredPayRunMilestoneView.ToTieredPayRunMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPayRunMilestoneView.ToTieredPayRunMilestoneViews(new List<TieredPayRunMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPayRunMilestoneViewShowsTheStatusInCapitals();
    }
}
