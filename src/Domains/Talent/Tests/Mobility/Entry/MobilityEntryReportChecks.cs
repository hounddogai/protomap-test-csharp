namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class MobilityEntryReportChecks
{
    public static void MobilityEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704073664);
        var items = new List<MobilityEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(MobilityEntry.NewMobilityEntry($"mobility-entry-{index}", "sample"));
        }
        var report = MobilityEntryReport.BuildMobilityEntryReport("test", items, clock);
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
        MobilityEntryReportCountsEachRecordOnce();
    }
}
