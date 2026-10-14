namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorProfileReportChecks
{
    public static void VisitorProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701060576);
        var items = new List<VisitorProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(VisitorProfile.NewVisitorProfile($"visitor-profile-{index}", "sample"));
        }
        var report = VisitorProfileReport.BuildVisitorProfileReport("test", items, clock);
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
        VisitorProfileReportCountsEachRecordOnce();
    }
}
