namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionMilestoneViewChecks
{
    public static void DeductionMilestoneViewShowsTheStatusInCapitals()
    {
        var item = DeductionMilestone.NewDeductionMilestone("code-928", "name-634");
        var view = DeductionMilestoneView.ToDeductionMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionMilestoneView.ToDeductionMilestoneViews(new List<DeductionMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionMilestoneViewShowsTheStatusInCapitals();
    }
}
