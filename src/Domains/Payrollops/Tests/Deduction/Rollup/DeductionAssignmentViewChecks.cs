namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionAssignmentViewChecks
{
    public static void DeductionAssignmentViewShowsTheStatusInCapitals()
    {
        var item = DeductionAssignment.NewDeductionAssignment("code-887", "title-87");
        var view = DeductionAssignmentView.ToDeductionAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionAssignmentView.ToDeductionAssignmentViews(new List<DeductionAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionAssignmentViewShowsTheStatusInCapitals();
    }
}
