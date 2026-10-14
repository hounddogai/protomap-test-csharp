namespace Acme.Workforce.Payrollops.Tests.Deduction.Cycle;

using Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DeductionCycleViewChecks
{
    public static void DeductionCycleViewShowsTheStatusInCapitals()
    {
        var item = DeductionCycle.NewDeductionCycle("code-764", "name-510");
        var view = DeductionCycleView.ToDeductionCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionCycleView.ToDeductionCycleViews(new List<DeductionCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionCycleViewShowsTheStatusInCapitals();
    }
}
