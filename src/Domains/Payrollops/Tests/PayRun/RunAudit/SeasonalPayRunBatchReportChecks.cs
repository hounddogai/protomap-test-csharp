namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class SeasonalPayRunBatchReportChecks
{
    public static void SeasonalPayRunBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706685361);
        var items = new List<SeasonalPayRunBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SeasonalPayRunBatch.NewSeasonalPayRunBatch($"seasonal-pay-run-batch-{index}", "sample"));
        }
        var report = SeasonalPayRunBatchReport.BuildSeasonalPayRunBatchReport("test", items, clock);
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
        SeasonalPayRunBatchReportCountsEachRecordOnce();
    }
}
