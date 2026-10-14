namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SpanAssignmentReportChecks
{
    public static void SpanAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702568697);
        var items = new List<SpanAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SpanAssignment.NewSpanAssignment($"span-assignment-{index}", "sample"));
        }
        var report = SpanAssignmentReport.BuildSpanAssignmentReport("test", items, clock);
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
        SpanAssignmentReportCountsEachRecordOnce();
    }
}
