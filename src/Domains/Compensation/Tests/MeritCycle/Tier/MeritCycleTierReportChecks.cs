namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleTierReportChecks
{
    public static void MeritCycleTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702220279);
        var items = new List<MeritCycleTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MeritCycleTier.NewMeritCycleTier($"merit-cycle-tier-{index}", "sample"));
        }
        var report = MeritCycleTierReport.BuildMeritCycleTierReport("test", items, clock);
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
        MeritCycleTierReportCountsEachRecordOnce();
    }
}
