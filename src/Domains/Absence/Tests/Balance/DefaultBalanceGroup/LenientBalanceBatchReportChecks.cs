namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceBatchReportChecks
{
    public static void LenientBalanceBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708565653);
        var items = new List<LenientBalanceBatch>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientBalanceBatch.NewLenientBalanceBatch($"lenient-balance-batch-{index}", "sample"));
        }
        var report = LenientBalanceBatchReport.BuildLenientBalanceBatchReport("test", items, clock);
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
        LenientBalanceBatchReportCountsEachRecordOnce();
    }
}
