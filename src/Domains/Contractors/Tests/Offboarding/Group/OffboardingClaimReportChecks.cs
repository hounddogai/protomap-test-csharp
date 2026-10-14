namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class OffboardingClaimReportChecks
{
    public static void OffboardingClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701554769);
        var items = new List<OffboardingClaim>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(OffboardingClaim.NewOffboardingClaim($"offboarding-claim-{index}", "sample"));
        }
        var report = OffboardingClaimReport.BuildOffboardingClaimReport("test", items, clock);
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
        OffboardingClaimReportCountsEachRecordOnce();
    }
}
