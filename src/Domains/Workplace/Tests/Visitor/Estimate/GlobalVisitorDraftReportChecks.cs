namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalVisitorDraftReportChecks
{
    public static void GlobalVisitorDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706306739);
        var items = new List<GlobalVisitorDraft>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalVisitorDraft.NewGlobalVisitorDraft($"global-visitor-draft-{index}", "sample"));
        }
        var report = GlobalVisitorDraftReport.BuildGlobalVisitorDraftReport("test", items, clock);
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
        GlobalVisitorDraftReportCountsEachRecordOnce();
    }
}
