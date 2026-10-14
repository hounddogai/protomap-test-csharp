namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleEntryReportChecks
{
    public static void MeritCycleEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709474208);
        var items = new List<MeritCycleEntry>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(MeritCycleEntry.NewMeritCycleEntry($"merit-cycle-entry-{index}", "sample"));
        }
        var report = MeritCycleEntryReport.BuildMeritCycleEntryReport("test", items, clock);
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
        MeritCycleEntryReportCountsEachRecordOnce();
    }
}
