namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredActionPlanBatchReportChecks
{
    public static void TieredActionPlanBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708499370);
        var items = new List<TieredActionPlanBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredActionPlanBatch.NewTieredActionPlanBatch($"tiered-action-plan-batch-{index}", "sample"));
        }
        var report = TieredActionPlanBatchReport.BuildTieredActionPlanBatchReport("test", items, clock);
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
        TieredActionPlanBatchReportCountsEachRecordOnce();
    }
}
