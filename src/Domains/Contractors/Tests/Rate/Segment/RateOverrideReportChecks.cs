namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateOverrideReportChecks
{
    public static void RateOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701243513);
        var items = new List<RateOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RateOverride.NewRateOverride($"rate-override-{index}", "sample"));
        }
        var report = RateOverrideReport.BuildRateOverrideReport("test", items, clock);
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
        RateOverrideReportCountsEachRecordOnce();
    }
}
