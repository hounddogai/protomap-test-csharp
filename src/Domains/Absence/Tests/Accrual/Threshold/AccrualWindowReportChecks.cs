namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualWindowReportChecks
{
    public static void AccrualWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704041082);
        var items = new List<AccrualWindow>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AccrualWindow.NewAccrualWindow($"accrual-window-{index}", "sample"));
        }
        var report = AccrualWindowReport.BuildAccrualWindowReport("test", items, clock);
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
        AccrualWindowReportCountsEachRecordOnce();
    }
}
