namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyBalanceAdjustmentReportChecks
{
    public static void LegacyBalanceAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703246414);
        var items = new List<LegacyBalanceAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacyBalanceAdjustment.NewLegacyBalanceAdjustment($"legacy-balance-adjustment-{index}", "sample"));
        }
        var report = LegacyBalanceAdjustmentReport.BuildLegacyBalanceAdjustmentReport("test", items, clock);
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
        LegacyBalanceAdjustmentReportCountsEachRecordOnce();
    }
}
