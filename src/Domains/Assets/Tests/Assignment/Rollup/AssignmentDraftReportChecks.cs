namespace Acme.Workforce.Assets.Tests.Assignment.Rollup;

using Acme.Workforce.Assets.Assignment.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AssignmentDraftReportChecks
{
    public static void AssignmentDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701750454);
        var items = new List<AssignmentDraft>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AssignmentDraft.NewAssignmentDraft($"assignment-draft-{index}", "sample"));
        }
        var report = AssignmentDraftReport.BuildAssignmentDraftReport("test", items, clock);
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
        AssignmentDraftReportCountsEachRecordOnce();
    }
}
