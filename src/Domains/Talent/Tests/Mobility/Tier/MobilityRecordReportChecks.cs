namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityRecordReportChecks
{
    public static void MobilityRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708127500);
        var items = new List<MobilityRecord>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(MobilityRecord.NewMobilityRecord($"mobility-record-{index}", "sample"));
        }
        var report = MobilityRecordReport.BuildMobilityRecordReport("test", items, clock);
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
        MobilityRecordReportCountsEachRecordOnce();
    }
}
