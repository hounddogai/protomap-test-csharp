namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdjustmentClaimReportChecks
{
    public static void AdjustmentClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703777980);
        var items = new List<AdjustmentClaim>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdjustmentClaim.NewAdjustmentClaim($"adjustment-claim-{index}", "sample"));
        }
        var report = AdjustmentClaimReport.BuildAdjustmentClaimReport("test", items, clock);
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
        AdjustmentClaimReportCountsEachRecordOnce();
    }
}
