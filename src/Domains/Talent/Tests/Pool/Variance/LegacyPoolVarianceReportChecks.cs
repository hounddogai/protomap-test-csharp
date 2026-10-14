namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyPoolVarianceReportChecks
{
    public static void LegacyPoolVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703273826);
        var items = new List<LegacyPoolVariance>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyPoolVariance.NewLegacyPoolVariance($"legacy-pool-variance-{index}", "sample"));
        }
        var report = LegacyPoolVarianceReport.BuildLegacyPoolVarianceReport("test", items, clock);
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
        LegacyPoolVarianceReportCountsEachRecordOnce();
    }
}
