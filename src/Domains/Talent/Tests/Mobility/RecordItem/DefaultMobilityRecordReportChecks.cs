namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultMobilityRecordReportChecks
{
    public static void DefaultMobilityRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705299176);
        var items = new List<DefaultMobilityRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultMobilityRecord.NewDefaultMobilityRecord($"default-mobility-record-{index}", "sample"));
        }
        var report = DefaultMobilityRecordReport.BuildDefaultMobilityRecordReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultMobilityRecordReportCountsEachRecordOnce();
    }
}
