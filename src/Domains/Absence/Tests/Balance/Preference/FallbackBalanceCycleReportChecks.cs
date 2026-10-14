namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FallbackBalanceCycleReportChecks
{
    public static void FallbackBalanceCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708057137);
        var items = new List<FallbackBalanceCycle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackBalanceCycle.NewFallbackBalanceCycle($"fallback-balance-cycle-{index}", "sample"));
        }
        var report = FallbackBalanceCycleReport.BuildFallbackBalanceCycleReport("test", items, clock);
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
        FallbackBalanceCycleReportCountsEachRecordOnce();
    }
}
