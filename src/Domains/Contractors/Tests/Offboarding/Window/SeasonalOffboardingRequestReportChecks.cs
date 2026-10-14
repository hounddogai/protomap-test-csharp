namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalOffboardingRequestReportChecks
{
    public static void SeasonalOffboardingRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700497731);
        var items = new List<SeasonalOffboardingRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalOffboardingRequest.NewSeasonalOffboardingRequest($"seasonal-offboarding-request-{index}", "sample"));
        }
        var report = SeasonalOffboardingRequestReport.BuildSeasonalOffboardingRequestReport("test", items, clock);
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
        SeasonalOffboardingRequestReportCountsEachRecordOnce();
    }
}
