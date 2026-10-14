namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PrimaryMeritCycleWindowReportChecks
{
    public static void PrimaryMeritCycleWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707692064);
        var items = new List<PrimaryMeritCycleWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow($"primary-merit-cycle-window-{index}", "sample"));
        }
        var report = PrimaryMeritCycleWindowReport.BuildPrimaryMeritCycleWindowReport("test", items, clock);
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
        PrimaryMeritCycleWindowReportCountsEachRecordOnce();
    }
}
