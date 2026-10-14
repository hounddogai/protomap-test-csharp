namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledBenchmarkScheduleReportChecks
{
    public static void ScheduledBenchmarkScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700817193);
        var items = new List<ScheduledBenchmarkSchedule>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule($"scheduled-benchmark-schedule-{index}", "sample"));
        }
        var report = ScheduledBenchmarkScheduleReport.BuildScheduledBenchmarkScheduleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkScheduleReportCountsEachRecordOnce();
    }
}
