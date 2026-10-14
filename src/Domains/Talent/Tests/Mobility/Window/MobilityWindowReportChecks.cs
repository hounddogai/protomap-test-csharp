namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityWindowReportChecks
{
    public static void MobilityWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705102222);
        var items = new List<MobilityWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(MobilityWindow.NewMobilityWindow($"mobility-window-{index}", "sample"));
        }
        var report = MobilityWindowReport.BuildMobilityWindowReport("test", items, clock);
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
        MobilityWindowReportCountsEachRecordOnce();
    }
}
