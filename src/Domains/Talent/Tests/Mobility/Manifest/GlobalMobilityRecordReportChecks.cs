namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class GlobalMobilityRecordReportChecks
{
    public static void GlobalMobilityRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706919776);
        var items = new List<GlobalMobilityRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalMobilityRecord.NewGlobalMobilityRecord($"global-mobility-record-{index}", "sample"));
        }
        var report = GlobalMobilityRecordReport.BuildGlobalMobilityRecordReport("test", items, clock);
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
        GlobalMobilityRecordReportCountsEachRecordOnce();
    }
}
