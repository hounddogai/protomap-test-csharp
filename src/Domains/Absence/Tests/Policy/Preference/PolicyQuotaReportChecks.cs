namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyQuotaReportChecks
{
    public static void PolicyQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708325771);
        var items = new List<PolicyQuota>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PolicyQuota.NewPolicyQuota($"policy-quota-{index}", "sample"));
        }
        var report = PolicyQuotaReport.BuildPolicyQuotaReport("test", items, clock);
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
        PolicyQuotaReportCountsEachRecordOnce();
    }
}
