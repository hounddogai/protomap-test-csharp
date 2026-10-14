namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedDeductionPlanViewChecks
{
    public static void WeightedDeductionPlanViewShowsTheStatusInCapitals()
    {
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("code-461", "label-666");
        var view = WeightedDeductionPlanView.ToWeightedDeductionPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedDeductionPlanView.ToWeightedDeductionPlanViews(new List<WeightedDeductionPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedDeductionPlanViewShowsTheStatusInCapitals();
    }
}
