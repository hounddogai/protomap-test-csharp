namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalPositionPolicyReportChecks
{
    public static void RegionalPositionPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708049576);
        var items = new List<RegionalPositionPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RegionalPositionPolicy.NewRegionalPositionPolicy($"regional-position-policy-{index}", "sample"));
        }
        var report = RegionalPositionPolicyReport.BuildRegionalPositionPolicyReport("test", items, clock);
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
        RegionalPositionPolicyReportCountsEachRecordOnce();
    }
}
