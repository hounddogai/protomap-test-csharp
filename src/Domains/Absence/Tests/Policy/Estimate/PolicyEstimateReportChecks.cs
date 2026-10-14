namespace Acme.Workforce.Absence.Tests.Policy.Estimate;

using Acme.Workforce.Absence.Policy.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PolicyEstimateReportChecks
{
    public static void PolicyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700397224);
        var items = new List<PolicyEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PolicyEstimate.NewPolicyEstimate($"policy-estimate-{index}", "sample"));
        }
        var report = PolicyEstimateReport.BuildPolicyEstimateReport("test", items, clock);
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
        PolicyEstimateReportCountsEachRecordOnce();
    }
}
