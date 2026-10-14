namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultHeadcountProfileReportChecks
{
    public static void DefaultHeadcountProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704467084);
        var items = new List<DefaultHeadcountProfile>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultHeadcountProfile.NewDefaultHeadcountProfile($"default-headcount-profile-{index}", "sample"));
        }
        var report = DefaultHeadcountProfileReport.BuildDefaultHeadcountProfileReport("test", items, clock);
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
        DefaultHeadcountProfileReportCountsEachRecordOnce();
    }
}
