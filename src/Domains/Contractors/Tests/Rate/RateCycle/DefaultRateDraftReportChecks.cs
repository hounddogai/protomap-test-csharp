namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class DefaultRateDraftReportChecks
{
    public static void DefaultRateDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708792299);
        var items = new List<DefaultRateDraft>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultRateDraft.NewDefaultRateDraft($"default-rate-draft-{index}", "sample"));
        }
        var report = DefaultRateDraftReport.BuildDefaultRateDraftReport("test", items, clock);
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
        DefaultRateDraftReportCountsEachRecordOnce();
    }
}
