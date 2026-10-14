namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LegacyRateVarianceReportChecks
{
    public static void LegacyRateVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704713585);
        var items = new List<LegacyRateVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyRateVariance.NewLegacyRateVariance($"legacy-rate-variance-{index}", "sample"));
        }
        var report = LegacyRateVarianceReport.BuildLegacyRateVarianceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LegacyRateVarianceReportCountsEachRecordOnce();
    }
}
