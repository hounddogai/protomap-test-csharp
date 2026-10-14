namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalRetroPayBatchReportChecks
{
    public static void GlobalRetroPayBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700849955);
        var items = new List<GlobalRetroPayBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalRetroPayBatch.NewGlobalRetroPayBatch($"global-retro-pay-batch-{index}", "sample"));
        }
        var report = GlobalRetroPayBatchReport.BuildGlobalRetroPayBatchReport("test", items, clock);
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
        GlobalRetroPayBatchReportCountsEachRecordOnce();
    }
}
