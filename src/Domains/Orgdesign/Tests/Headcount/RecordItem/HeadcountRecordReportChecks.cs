namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountRecordReportChecks
{
    public static void HeadcountRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708720746);
        var items = new List<HeadcountRecord>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(HeadcountRecord.NewHeadcountRecord($"headcount-record-{index}", "sample"));
        }
        var report = HeadcountRecordReport.BuildHeadcountRecordReport("test", items, clock);
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
        HeadcountRecordReportCountsEachRecordOnce();
    }
}
