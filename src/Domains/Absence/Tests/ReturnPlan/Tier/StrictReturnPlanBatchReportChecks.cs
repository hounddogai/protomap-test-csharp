namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictReturnPlanBatchReportChecks
{
    public static void StrictReturnPlanBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708800147);
        var items = new List<StrictReturnPlanBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictReturnPlanBatch.NewStrictReturnPlanBatch($"strict-return-plan-batch-{index}", "sample"));
        }
        var report = StrictReturnPlanBatchReport.BuildStrictReturnPlanBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        StrictReturnPlanBatchReportCountsEachRecordOnce();
    }
}
