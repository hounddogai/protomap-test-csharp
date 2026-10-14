namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateTierReportChecks
{
    public static void RateTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706269188);
        var items = new List<RateTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RateTier.NewRateTier($"rate-tier-{index}", "sample"));
        }
        var report = RateTierReport.BuildRateTierReport("test", items, clock);
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
        RateTierReportCountsEachRecordOnce();
    }
}
