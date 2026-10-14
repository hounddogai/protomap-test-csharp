namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyTemplateReportChecks
{
    public static void WarrantyTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703063700);
        var items = new List<WarrantyTemplate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(WarrantyTemplate.NewWarrantyTemplate($"warranty-template-{index}", "sample"));
        }
        var report = WarrantyTemplateReport.BuildWarrantyTemplateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WarrantyTemplateReportCountsEachRecordOnce();
    }
}
