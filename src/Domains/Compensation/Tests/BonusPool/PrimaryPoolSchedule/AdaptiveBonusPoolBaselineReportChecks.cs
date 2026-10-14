namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolBaselineReportChecks
{
    public static void AdaptiveBonusPoolBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702884517);
        var items = new List<AdaptiveBonusPoolBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline($"adaptive-bonus-pool-baseline-{index}", "sample"));
        }
        var report = AdaptiveBonusPoolBaselineReport.BuildAdaptiveBonusPoolBaselineReport("test", items, clock);
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
        AdaptiveBonusPoolBaselineReportCountsEachRecordOnce();
    }
}
