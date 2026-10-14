namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RegionalDeductionScheduleViewChecks
{
    public static void RegionalDeductionScheduleViewShowsTheStatusInCapitals()
    {
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("code-650", "label-800");
        var view = RegionalDeductionScheduleView.ToRegionalDeductionScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalDeductionScheduleView.ToRegionalDeductionScheduleViews(new List<RegionalDeductionSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalDeductionScheduleViewShowsTheStatusInCapitals();
    }
}
