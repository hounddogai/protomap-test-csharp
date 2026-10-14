namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccrualBatchReportChecks
{
    public static void TieredAccrualBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709368619);
        var items = new List<TieredAccrualBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredAccrualBatch.NewTieredAccrualBatch($"tiered-accrual-batch-{index}", "sample"));
        }
        var report = TieredAccrualBatchReport.BuildTieredAccrualBatchReport("test", items, clock);
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
        TieredAccrualBatchReportCountsEachRecordOnce();
    }
}
