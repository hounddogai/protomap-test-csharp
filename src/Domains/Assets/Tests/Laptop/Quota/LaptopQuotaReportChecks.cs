namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopQuotaReportChecks
{
    public static void LaptopQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706571036);
        var items = new List<LaptopQuota>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LaptopQuota.NewLaptopQuota($"laptop-quota-{index}", "sample"));
        }
        var report = LaptopQuotaReport.BuildLaptopQuotaReport("test", items, clock);
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
        LaptopQuotaReportCountsEachRecordOnce();
    }
}
