namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateAllocationReportChecks
{
    public static void RateAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707190048);
        var items = new List<RateAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RateAllocation.NewRateAllocation($"rate-allocation-{index}", "sample"));
        }
        var report = RateAllocationReport.BuildRateAllocationReport("test", items, clock);
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
        RateAllocationReportCountsEachRecordOnce();
    }
}
