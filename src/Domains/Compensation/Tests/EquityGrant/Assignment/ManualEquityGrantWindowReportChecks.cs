namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ManualEquityGrantWindowReportChecks
{
    public static void ManualEquityGrantWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709652214);
        var items = new List<ManualEquityGrantWindow>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualEquityGrantWindow.NewManualEquityGrantWindow($"manual-equity-grant-window-{index}", "sample"));
        }
        var report = ManualEquityGrantWindowReport.BuildManualEquityGrantWindowReport("test", items, clock);
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
        ManualEquityGrantWindowReportCountsEachRecordOnce();
    }
}
