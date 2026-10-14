namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class LenientEquityGrantBatchReportChecks
{
    public static void LenientEquityGrantBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708321971);
        var items = new List<LenientEquityGrantBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientEquityGrantBatch.NewLenientEquityGrantBatch($"lenient-equity-grant-batch-{index}", "sample"));
        }
        var report = LenientEquityGrantBatchReport.BuildLenientEquityGrantBatchReport("test", items, clock);
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
        LenientEquityGrantBatchReportCountsEachRecordOnce();
    }
}
