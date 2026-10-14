namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class VisitorWindowReportChecks
{
    public static void VisitorWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702104748);
        var items = new List<VisitorWindow>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(VisitorWindow.NewVisitorWindow($"visitor-window-{index}", "sample"));
        }
        var report = VisitorWindowReport.BuildVisitorWindowReport("test", items, clock);
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
        VisitorWindowReportCountsEachRecordOnce();
    }
}
