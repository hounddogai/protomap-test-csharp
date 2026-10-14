namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class PendingSuccessionPlanTargetViewChecks
{
    public static void PendingSuccessionPlanTargetViewShowsTheStatusInCapitals()
    {
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("code-335", "label-582");
        var view = PendingSuccessionPlanTargetView.ToPendingSuccessionPlanTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingSuccessionPlanTargetView.ToPendingSuccessionPlanTargetViews(new List<PendingSuccessionPlanTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingSuccessionPlanTargetViewShowsTheStatusInCapitals();
    }
}
