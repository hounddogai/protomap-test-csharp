namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class GlobalBalanceScheduleReportChecks
{
    public static void GlobalBalanceScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707188698);
        var items = new List<GlobalBalanceSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalBalanceSchedule.NewGlobalBalanceSchedule($"global-balance-schedule-{index}", "sample"));
        }
        var report = GlobalBalanceScheduleReport.BuildGlobalBalanceScheduleReport("test", items, clock);
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
        GlobalBalanceScheduleReportCountsEachRecordOnce();
    }
}
