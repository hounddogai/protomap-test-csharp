namespace Acme.Workforce.Compensation.Tests.MeritCycle.Claim;

using Acme.Workforce.Compensation.MeritCycle.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class FallbackMeritCycleClaimReportChecks
{
    public static void FallbackMeritCycleClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705951629);
        var items = new List<FallbackMeritCycleClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FallbackMeritCycleClaim.NewFallbackMeritCycleClaim($"fallback-merit-cycle-claim-{index}", "sample"));
        }
        var report = FallbackMeritCycleClaimReport.BuildFallbackMeritCycleClaimReport("test", items, clock);
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
        FallbackMeritCycleClaimReportCountsEachRecordOnce();
    }
}
