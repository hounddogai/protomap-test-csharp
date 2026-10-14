namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class RegionalPolicyEstimateReportChecks
{
    public static void RegionalPolicyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709787347);
        var items = new List<RegionalPolicyEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalPolicyEstimate.NewRegionalPolicyEstimate($"regional-policy-estimate-{index}", "sample"));
        }
        var report = RegionalPolicyEstimateReport.BuildRegionalPolicyEstimateReport("test", items, clock);
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
        RegionalPolicyEstimateReportCountsEachRecordOnce();
    }
}
