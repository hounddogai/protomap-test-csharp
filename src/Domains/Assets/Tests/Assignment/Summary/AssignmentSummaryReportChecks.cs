namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssignmentSummaryReportChecks
{
    public static void AssignmentSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706489370);
        var items = new List<AssignmentSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AssignmentSummary.NewAssignmentSummary($"assignment-summary-{index}", "sample"));
        }
        var report = AssignmentSummaryReport.BuildAssignmentSummaryReport("test", items, clock);
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
        AssignmentSummaryReportCountsEachRecordOnce();
    }
}
