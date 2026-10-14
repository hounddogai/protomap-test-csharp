namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanBatchReportChecks
{
    public static void SuccessionPlanBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708604275);
        var items = new List<SuccessionPlanBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SuccessionPlanBatch.NewSuccessionPlanBatch($"succession-plan-batch-{index}", "sample"));
        }
        var report = SuccessionPlanBatchReport.BuildSuccessionPlanBatchReport("test", items, clock);
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
        SuccessionPlanBatchReportCountsEachRecordOnce();
    }
}
