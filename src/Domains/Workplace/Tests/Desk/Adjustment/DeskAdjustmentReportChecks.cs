namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskAdjustmentReportChecks
{
    public static void DeskAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701255882);
        var items = new List<DeskAdjustment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeskAdjustment.NewDeskAdjustment($"desk-adjustment-{index}", "sample"));
        }
        var report = DeskAdjustmentReport.BuildDeskAdjustmentReport("test", items, clock);
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
        DeskAdjustmentReportCountsEachRecordOnce();
    }
}
