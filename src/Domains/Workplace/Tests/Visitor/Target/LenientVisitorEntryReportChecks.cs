namespace Acme.Workforce.Workplace.Tests.Visitor.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientVisitorEntryReportChecks
{
    public static void LenientVisitorEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709847585);
        var items = new List<LenientVisitorEntry>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientVisitorEntry.NewLenientVisitorEntry($"lenient-visitor-entry-{index}", "sample"));
        }
        var report = LenientVisitorEntryReport.BuildLenientVisitorEntryReport("test", items, clock);
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
        LenientVisitorEntryReportCountsEachRecordOnce();
    }
}
