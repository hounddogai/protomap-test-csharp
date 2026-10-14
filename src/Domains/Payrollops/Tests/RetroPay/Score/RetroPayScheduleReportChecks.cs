namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RetroPayScheduleReportChecks
{
    public static void RetroPayScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701929602);
        var items = new List<RetroPaySchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RetroPaySchedule.NewRetroPaySchedule($"retro-pay-schedule-{index}", "sample"));
        }
        var report = RetroPayScheduleReport.BuildRetroPayScheduleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RetroPayScheduleReportCountsEachRecordOnce();
    }
}
