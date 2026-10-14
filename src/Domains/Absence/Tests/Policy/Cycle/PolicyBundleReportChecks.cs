namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyBundleReportChecks
{
    public static void PolicyBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700935361);
        var items = new List<PolicyBundle>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PolicyBundle.NewPolicyBundle($"policy-bundle-{index}", "sample"));
        }
        var report = PolicyBundleReport.BuildPolicyBundleReport("test", items, clock);
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
        PolicyBundleReportCountsEachRecordOnce();
    }
}
