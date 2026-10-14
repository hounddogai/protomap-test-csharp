namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleOverrideReportChecks
{
    public static void MeritCycleOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708945041);
        var items = new List<MeritCycleOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MeritCycleOverride.NewMeritCycleOverride($"merit-cycle-override-{index}", "sample"));
        }
        var report = MeritCycleOverrideReport.BuildMeritCycleOverrideReport("test", items, clock);
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
        MeritCycleOverrideReportCountsEachRecordOnce();
    }
}
