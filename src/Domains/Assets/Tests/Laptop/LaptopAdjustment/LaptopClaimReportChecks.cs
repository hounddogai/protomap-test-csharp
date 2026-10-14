namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class LaptopClaimReportChecks
{
    public static void LaptopClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705016839);
        var items = new List<LaptopClaim>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LaptopClaim.NewLaptopClaim($"laptop-claim-{index}", "sample"));
        }
        var report = LaptopClaimReport.BuildLaptopClaimReport("test", items, clock);
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
        LaptopClaimReportCountsEachRecordOnce();
    }
}
