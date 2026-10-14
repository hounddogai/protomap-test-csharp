namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateMappingReportChecks
{
    public static void RateMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707301610);
        var items = new List<RateMapping>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RateMapping.NewRateMapping($"rate-mapping-{index}", "sample"));
        }
        var report = RateMappingReport.BuildRateMappingReport("test", items, clock);
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
        RateMappingReportCountsEachRecordOnce();
    }
}
