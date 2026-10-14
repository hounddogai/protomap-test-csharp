namespace Acme.Workforce.Payrollops.Tests.PayRun.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PayRunScheduleReportChecks
{
    public static void PayRunScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709704448);
        var items = new List<PayRunSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayRunSchedule.NewPayRunSchedule($"pay-run-schedule-{index}", "sample"));
        }
        var report = PayRunScheduleReport.BuildPayRunScheduleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PayRunScheduleReportCountsEachRecordOnce();
    }
}
