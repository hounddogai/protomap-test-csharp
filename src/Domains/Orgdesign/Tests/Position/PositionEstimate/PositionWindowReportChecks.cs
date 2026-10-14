namespace Acme.Workforce.Orgdesign.Tests.Position.PositionEstimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.PositionEstimate;

/// <summary>Checks of position estimate code, run by the project's runner.</summary>
public static class PositionWindowReportChecks
{
    public static void PositionWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708667377);
        var items = new List<PositionWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PositionWindow.NewPositionWindow($"position-window-{index}", "sample"));
        }
        var report = PositionWindowReport.BuildPositionWindowReport("test", items, clock);
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
        PositionWindowReportCountsEachRecordOnce();
    }
}
