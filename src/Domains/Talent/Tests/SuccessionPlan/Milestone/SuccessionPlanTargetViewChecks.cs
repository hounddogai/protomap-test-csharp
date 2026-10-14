namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanTargetViewChecks
{
    public static void SuccessionPlanTargetViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("code-377", "name-459");
        var view = SuccessionPlanTargetView.ToSuccessionPlanTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanTargetView.ToSuccessionPlanTargetViews(new List<SuccessionPlanTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanTargetViewShowsTheStatusInCapitals();
    }
}
