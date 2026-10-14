namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionScheduleViewChecks
{
    public static void DeductionScheduleViewShowsTheStatusInCapitals()
    {
        var item = DeductionSchedule.NewDeductionSchedule("code-584", "title-743");
        var view = DeductionScheduleView.ToDeductionScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionScheduleView.ToDeductionScheduleViews(new List<DeductionSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionScheduleViewShowsTheStatusInCapitals();
    }
}
