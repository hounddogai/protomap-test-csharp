namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentEntryReportChecks
{
    public static void AssignmentEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709233399);
        var items = new List<AssignmentEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AssignmentEntry.NewAssignmentEntry($"assignment-entry-{index}", "sample"));
        }
        var report = AssignmentEntryReport.BuildAssignmentEntryReport("test", items, clock);
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
        AssignmentEntryReportCountsEachRecordOnce();
    }
}
