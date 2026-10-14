namespace Acme.Workforce.Absence.Tests.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedBalanceOverrideReportChecks
{
    public static void BatchedBalanceOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703774185);
        var items = new List<BatchedBalanceOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BatchedBalanceOverride.NewBatchedBalanceOverride($"batched-balance-override-{index}", "sample"));
        }
        var report = BatchedBalanceOverrideReport.BuildBatchedBalanceOverrideReport("test", items, clock);
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
        BatchedBalanceOverrideReportCountsEachRecordOnce();
    }
}
