namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class WeightedReturnPlanOverrideViewChecks
{
    public static void WeightedReturnPlanOverrideViewShowsTheStatusInCapitals()
    {
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("code-322", "label-710");
        var view = WeightedReturnPlanOverrideView.ToWeightedReturnPlanOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedReturnPlanOverrideView.ToWeightedReturnPlanOverrideViews(new List<WeightedReturnPlanOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedReturnPlanOverrideViewShowsTheStatusInCapitals();
    }
}
