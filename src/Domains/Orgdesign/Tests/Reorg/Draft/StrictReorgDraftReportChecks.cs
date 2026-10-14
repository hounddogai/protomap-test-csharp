namespace Acme.Workforce.Orgdesign.Tests.Reorg.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictReorgDraftReportChecks
{
    public static void StrictReorgDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707413959);
        var items = new List<StrictReorgDraft>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictReorgDraft.NewStrictReorgDraft($"strict-reorg-draft-{index}", "sample"));
        }
        var report = StrictReorgDraftReport.BuildStrictReorgDraftReport("test", items, clock);
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
        StrictReorgDraftReportCountsEachRecordOnce();
    }
}
