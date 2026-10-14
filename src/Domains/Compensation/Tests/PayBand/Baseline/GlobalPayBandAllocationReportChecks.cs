namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalPayBandAllocationReportChecks
{
    public static void GlobalPayBandAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703031697);
        var items = new List<GlobalPayBandAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalPayBandAllocation.NewGlobalPayBandAllocation($"global-pay-band-allocation-{index}", "sample"));
        }
        var report = GlobalPayBandAllocationReport.BuildGlobalPayBandAllocationReport("test", items, clock);
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
        GlobalPayBandAllocationReportCountsEachRecordOnce();
    }
}
