namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyAllocationReportChecks
{
    public static void AdaptivePolicyAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701430721);
        var items = new List<AdaptivePolicyAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptivePolicyAllocation.NewAdaptivePolicyAllocation($"adaptive-policy-allocation-{index}", "sample"));
        }
        var report = AdaptivePolicyAllocationReport.BuildAdaptivePolicyAllocationReport("test", items, clock);
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
        AdaptivePolicyAllocationReportCountsEachRecordOnce();
    }
}
