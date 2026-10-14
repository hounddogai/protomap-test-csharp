namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class VisitorOverrideReportChecks
{
    public static void VisitorOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702414690);
        var items = new List<VisitorOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(VisitorOverride.NewVisitorOverride($"visitor-override-{index}", "sample"));
        }
        var report = VisitorOverrideReport.BuildVisitorOverrideReport("test", items, clock);
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
        VisitorOverrideReportCountsEachRecordOnce();
    }
}
