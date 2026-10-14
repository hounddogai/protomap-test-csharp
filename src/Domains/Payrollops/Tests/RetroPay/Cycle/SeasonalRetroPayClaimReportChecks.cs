namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class SeasonalRetroPayClaimReportChecks
{
    public static void SeasonalRetroPayClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702547972);
        var items = new List<SeasonalRetroPayClaim>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SeasonalRetroPayClaim.NewSeasonalRetroPayClaim($"seasonal-retro-pay-claim-{index}", "sample"));
        }
        var report = SeasonalRetroPayClaimReport.BuildSeasonalRetroPayClaimReport("test", items, clock);
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
        SeasonalRetroPayClaimReportCountsEachRecordOnce();
    }
}
