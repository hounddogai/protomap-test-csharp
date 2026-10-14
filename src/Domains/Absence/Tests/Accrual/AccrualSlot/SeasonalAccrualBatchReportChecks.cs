namespace Acme.Workforce.Absence.Tests.Accrual.AccrualSlot;

using Acme.Workforce.Absence.Accrual.AccrualSlot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of accrual slot code, run by the project's runner.</summary>
public static class SeasonalAccrualBatchReportChecks
{
    public static void SeasonalAccrualBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708648155);
        var items = new List<SeasonalAccrualBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalAccrualBatch.NewSeasonalAccrualBatch($"seasonal-accrual-batch-{index}", "sample"));
        }
        var report = SeasonalAccrualBatchReport.BuildSeasonalAccrualBatchReport("test", items, clock);
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
        SeasonalAccrualBatchReportCountsEachRecordOnce();
    }
}
