namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopTemplateReportChecks
{
    public static void LaptopTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707326355);
        var items = new List<LaptopTemplate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LaptopTemplate.NewLaptopTemplate($"laptop-template-{index}", "sample"));
        }
        var report = LaptopTemplateReport.BuildLaptopTemplateReport("test", items, clock);
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
        LaptopTemplateReportCountsEachRecordOnce();
    }
}
