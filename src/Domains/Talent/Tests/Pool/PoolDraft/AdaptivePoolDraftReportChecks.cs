namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class AdaptivePoolDraftReportChecks
{
    public static void AdaptivePoolDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707172419);
        var items = new List<AdaptivePoolDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptivePoolDraft.NewAdaptivePoolDraft($"adaptive-pool-draft-{index}", "sample"));
        }
        var report = AdaptivePoolDraftReport.BuildAdaptivePoolDraftReport("test", items, clock);
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
        AdaptivePoolDraftReportCountsEachRecordOnce();
    }
}
