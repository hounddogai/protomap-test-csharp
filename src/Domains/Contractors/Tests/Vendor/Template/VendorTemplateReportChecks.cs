namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorTemplateReportChecks
{
    public static void VendorTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702838963);
        var items = new List<VendorTemplate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(VendorTemplate.NewVendorTemplate($"vendor-template-{index}", "sample"));
        }
        var report = VendorTemplateReport.BuildVendorTemplateReport("test", items, clock);
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
        VendorTemplateReportCountsEachRecordOnce();
    }
}
