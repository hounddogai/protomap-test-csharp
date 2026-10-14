namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class PrimaryVisitorOverrideReportChecks
{
    public static void PrimaryVisitorOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703216274);
        var items = new List<PrimaryVisitorOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryVisitorOverride.NewPrimaryVisitorOverride($"primary-visitor-override-{index}", "sample"));
        }
        var report = PrimaryVisitorOverrideReport.BuildPrimaryVisitorOverrideReport("test", items, clock);
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
        PrimaryVisitorOverrideReportCountsEachRecordOnce();
    }
}
