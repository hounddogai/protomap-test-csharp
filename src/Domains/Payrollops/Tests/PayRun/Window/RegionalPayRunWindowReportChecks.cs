namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalPayRunWindowReportChecks
{
    public static void RegionalPayRunWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702506239);
        var items = new List<RegionalPayRunWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RegionalPayRunWindow.NewRegionalPayRunWindow($"regional-pay-run-window-{index}", "sample"));
        }
        var report = RegionalPayRunWindowReport.BuildRegionalPayRunWindowReport("test", items, clock);
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
        RegionalPayRunWindowReportCountsEachRecordOnce();
    }
}
