namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingPolicyReportChecks
{
    public static void OffboardingPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705704882);
        var items = new List<OffboardingPolicy>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(OffboardingPolicy.NewOffboardingPolicy($"offboarding-policy-{index}", "sample"));
        }
        var report = OffboardingPolicyReport.BuildOffboardingPolicyReport("test", items, clock);
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
        OffboardingPolicyReportCountsEachRecordOnce();
    }
}
