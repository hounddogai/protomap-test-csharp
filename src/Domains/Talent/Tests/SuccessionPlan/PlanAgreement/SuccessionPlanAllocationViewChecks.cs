namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class SuccessionPlanAllocationViewChecks
{
    public static void SuccessionPlanAllocationViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("code-93", "label-548");
        var view = SuccessionPlanAllocationView.ToSuccessionPlanAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanAllocationView.ToSuccessionPlanAllocationViews(new List<SuccessionPlanAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanAllocationViewShowsTheStatusInCapitals();
    }
}
