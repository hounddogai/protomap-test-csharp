namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientPoolDraftReportChecks
{
    public static void LenientPoolDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704747658);
        var items = new List<LenientPoolDraft>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientPoolDraft.NewLenientPoolDraft($"lenient-pool-draft-{index}", "sample"));
        }
        var report = LenientPoolDraftReport.BuildLenientPoolDraftReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientPoolDraftReportCountsEachRecordOnce();
    }
}
