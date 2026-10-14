namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityAllocationReportChecks
{
    public static void MobilityAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704099428);
        var items = new List<MobilityAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MobilityAllocation.NewMobilityAllocation($"mobility-allocation-{index}", "sample"));
        }
        var report = MobilityAllocationReport.BuildMobilityAllocationReport("test", items, clock);
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
        MobilityAllocationReportCountsEachRecordOnce();
    }
}
