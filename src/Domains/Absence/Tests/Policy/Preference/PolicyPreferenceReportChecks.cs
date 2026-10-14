namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyPreferenceReportChecks
{
    public static void PolicyPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703990959);
        var items = new List<PolicyPreference>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PolicyPreference.NewPolicyPreference($"policy-preference-{index}", "sample"));
        }
        var report = PolicyPreferenceReport.BuildPolicyPreferenceReport("test", items, clock);
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
        PolicyPreferenceReportCountsEachRecordOnce();
    }
}
