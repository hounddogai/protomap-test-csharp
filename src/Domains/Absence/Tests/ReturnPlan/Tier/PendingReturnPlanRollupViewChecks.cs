namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingReturnPlanRollupViewChecks
{
    public static void PendingReturnPlanRollupViewShowsTheStatusInCapitals()
    {
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("code-562", "title-355");
        var view = PendingReturnPlanRollupView.ToPendingReturnPlanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingReturnPlanRollupView.ToPendingReturnPlanRollupViews(new List<PendingReturnPlanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingReturnPlanRollupViewShowsTheStatusInCapitals();
    }
}
