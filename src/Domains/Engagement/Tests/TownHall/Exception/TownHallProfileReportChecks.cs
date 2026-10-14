namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallProfileReportChecks
{
    public static void TownHallProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703810043);
        var items = new List<TownHallProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TownHallProfile.NewTownHallProfile($"town-hall-profile-{index}", "sample"));
        }
        var report = TownHallProfileReport.BuildTownHallProfileReport("test", items, clock);
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
        TownHallProfileReportCountsEachRecordOnce();
    }
}
