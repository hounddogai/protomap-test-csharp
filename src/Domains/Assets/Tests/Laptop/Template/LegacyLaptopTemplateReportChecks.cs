namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyLaptopTemplateReportChecks
{
    public static void LegacyLaptopTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709807947);
        var items = new List<LegacyLaptopTemplate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyLaptopTemplate.NewLegacyLaptopTemplate($"legacy-laptop-template-{index}", "sample"));
        }
        var report = LegacyLaptopTemplateReport.BuildLegacyLaptopTemplateReport("test", items, clock);
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
        LegacyLaptopTemplateReportCountsEachRecordOnce();
    }
}
