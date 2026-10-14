namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskRecordReportChecks
{
    public static void DeskRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707094801);
        var items = new List<DeskRecord>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeskRecord.NewDeskRecord($"desk-record-{index}", "sample"));
        }
        var report = DeskRecordReport.BuildDeskRecordReport("test", items, clock);
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
        DeskRecordReportCountsEachRecordOnce();
    }
}
