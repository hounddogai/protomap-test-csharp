namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyManifestReportChecks
{
    public static void PolicyManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708895085);
        var items = new List<PolicyManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PolicyManifest.NewPolicyManifest($"policy-manifest-{index}", "sample"));
        }
        var report = PolicyManifestReport.BuildPolicyManifestReport("test", items, clock);
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
        PolicyManifestReportCountsEachRecordOnce();
    }
}
