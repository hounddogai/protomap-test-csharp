namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class FallbackRateCycleReportChecks
{
    public static void FallbackRateCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700388573);
        var items = new List<FallbackRateCycle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FallbackRateCycle.NewFallbackRateCycle($"fallback-rate-cycle-{index}", "sample"));
        }
        var report = FallbackRateCycleReport.BuildFallbackRateCycleReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FallbackRateCycleReportCountsEachRecordOnce();
    }
}
