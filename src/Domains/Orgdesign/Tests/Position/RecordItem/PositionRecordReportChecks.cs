namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionRecordReportChecks
{
    public static void PositionRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701993251);
        var items = new List<PositionRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PositionRecord.NewPositionRecord($"position-record-{index}", "sample"));
        }
        var report = PositionRecordReport.BuildPositionRecordReport("test", items, clock);
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
        PositionRecordReportCountsEachRecordOnce();
    }
}
