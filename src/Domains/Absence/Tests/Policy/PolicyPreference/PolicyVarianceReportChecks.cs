namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class PolicyVarianceReportChecks
{
    public static void PolicyVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702671893);
        var items = new List<PolicyVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PolicyVariance.NewPolicyVariance($"policy-variance-{index}", "sample"));
        }
        var report = PolicyVarianceReport.BuildPolicyVarianceReport("test", items, clock);
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
        PolicyVarianceReportCountsEachRecordOnce();
    }
}
