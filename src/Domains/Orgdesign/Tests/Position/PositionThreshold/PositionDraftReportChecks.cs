namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PositionDraftReportChecks
{
    public static void PositionDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708326292);
        var items = new List<PositionDraft>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PositionDraft.NewPositionDraft($"position-draft-{index}", "sample"));
        }
        var report = PositionDraftReport.BuildPositionDraftReport("test", items, clock);
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
        PositionDraftReportCountsEachRecordOnce();
    }
}
