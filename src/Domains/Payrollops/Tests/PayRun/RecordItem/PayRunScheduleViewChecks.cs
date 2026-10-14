namespace Acme.Workforce.Payrollops.Tests.PayRun.RecordItem;

using Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PayRunScheduleViewChecks
{
    public static void PayRunScheduleViewShowsTheStatusInCapitals()
    {
        var item = PayRunSchedule.NewPayRunSchedule("code-555", "name-601");
        var view = PayRunScheduleView.ToPayRunScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunScheduleView.ToPayRunScheduleViews(new List<PayRunSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunScheduleViewShowsTheStatusInCapitals();
    }
}
