namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GlobalOffboardingRequestReportChecks
{
    public static void GlobalOffboardingRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703463352);
        var items = new List<GlobalOffboardingRequest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalOffboardingRequest.NewGlobalOffboardingRequest($"global-offboarding-request-{index}", "sample"));
        }
        var report = GlobalOffboardingRequestReport.BuildGlobalOffboardingRequestReport("test", items, clock);
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
        GlobalOffboardingRequestReportCountsEachRecordOnce();
    }
}
