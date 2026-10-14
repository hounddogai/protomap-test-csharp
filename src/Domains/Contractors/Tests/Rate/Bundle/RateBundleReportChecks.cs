namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RateBundleReportChecks
{
    public static void RateBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708816508);
        var items = new List<RateBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RateBundle.NewRateBundle($"rate-bundle-{index}", "sample"));
        }
        var report = RateBundleReport.BuildRateBundleReport("test", items, clock);
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
        RateBundleReportCountsEachRecordOnce();
    }
}
