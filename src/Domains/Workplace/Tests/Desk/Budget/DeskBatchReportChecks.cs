namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeskBatchReportChecks
{
    public static void DeskBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709115378);
        var items = new List<DeskBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DeskBatch.NewDeskBatch($"desk-batch-{index}", "sample"));
        }
        var report = DeskBatchReport.BuildDeskBatchReport("test", items, clock);
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
        DeskBatchReportCountsEachRecordOnce();
    }
}
