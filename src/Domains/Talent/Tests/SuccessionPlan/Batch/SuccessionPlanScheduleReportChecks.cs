namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanScheduleReportChecks
{
    public static void SuccessionPlanScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702407395);
        var items = new List<SuccessionPlanSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SuccessionPlanSchedule.NewSuccessionPlanSchedule($"succession-plan-schedule-{index}", "sample"));
        }
        var report = SuccessionPlanScheduleReport.BuildSuccessionPlanScheduleReport("test", items, clock);
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
        SuccessionPlanScheduleReportCountsEachRecordOnce();
    }
}
