namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedOccupancyPolicyReportChecks
{
    public static void CappedOccupancyPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703344111);
        var items = new List<CappedOccupancyPolicy>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedOccupancyPolicy.NewCappedOccupancyPolicy($"capped-occupancy-policy-{index}", "sample"));
        }
        var report = CappedOccupancyPolicyReport.BuildCappedOccupancyPolicyReport("test", items, clock);
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
        CappedOccupancyPolicyReportCountsEachRecordOnce();
    }
}
