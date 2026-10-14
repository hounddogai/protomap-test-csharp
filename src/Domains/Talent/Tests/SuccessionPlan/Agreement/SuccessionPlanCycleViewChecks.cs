namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Agreement;

using Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SuccessionPlanCycleViewChecks
{
    public static void SuccessionPlanCycleViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("code-221", "label-18");
        var view = SuccessionPlanCycleView.ToSuccessionPlanCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanCycleView.ToSuccessionPlanCycleViews(new List<SuccessionPlanCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanCycleViewShowsTheStatusInCapitals();
    }
}
