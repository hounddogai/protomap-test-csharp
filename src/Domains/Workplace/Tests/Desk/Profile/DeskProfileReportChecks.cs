namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskProfileReportChecks
{
    public static void DeskProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703950603);
        var items = new List<DeskProfile>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DeskProfile.NewDeskProfile($"desk-profile-{index}", "sample"));
        }
        var report = DeskProfileReport.BuildDeskProfileReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DeskProfileReportCountsEachRecordOnce();
    }
}
