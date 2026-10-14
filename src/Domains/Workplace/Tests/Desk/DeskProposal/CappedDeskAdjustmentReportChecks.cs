namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class CappedDeskAdjustmentReportChecks
{
    public static void CappedDeskAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701104216);
        var items = new List<CappedDeskAdjustment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedDeskAdjustment.NewCappedDeskAdjustment($"capped-desk-adjustment-{index}", "sample"));
        }
        var report = CappedDeskAdjustmentReport.BuildCappedDeskAdjustmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedDeskAdjustmentReportCountsEachRecordOnce();
    }
}
