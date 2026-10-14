namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RegionalDeductionScheduleReportChecks
{
    public static void RegionalDeductionScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700214405);
        var items = new List<RegionalDeductionSchedule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalDeductionSchedule.NewRegionalDeductionSchedule($"regional-deduction-schedule-{index}", "sample"));
        }
        var report = RegionalDeductionScheduleReport.BuildRegionalDeductionScheduleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RegionalDeductionScheduleReportCountsEachRecordOnce();
    }
}
