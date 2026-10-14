namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class WeightedMobilityRecordReportChecks
{
    public static void WeightedMobilityRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709532845);
        var items = new List<WeightedMobilityRecord>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(WeightedMobilityRecord.NewWeightedMobilityRecord($"weighted-mobility-record-{index}", "sample"));
        }
        var report = WeightedMobilityRecordReport.BuildWeightedMobilityRecordReport("test", items, clock);
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
        WeightedMobilityRecordReportCountsEachRecordOnce();
    }
}
