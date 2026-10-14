namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PositionScheduleReportChecks
{
    public static void PositionScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708959973);
        var items = new List<PositionSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PositionSchedule.NewPositionSchedule($"position-schedule-{index}", "sample"));
        }
        var report = PositionScheduleReport.BuildPositionScheduleReport("test", items, clock);
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
        PositionScheduleReportCountsEachRecordOnce();
    }
}
