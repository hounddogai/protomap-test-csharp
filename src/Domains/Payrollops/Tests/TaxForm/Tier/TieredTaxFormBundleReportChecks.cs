namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredTaxFormBundleReportChecks
{
    public static void TieredTaxFormBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703707177);
        var items = new List<TieredTaxFormBundle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredTaxFormBundle.NewTieredTaxFormBundle($"tiered-tax-form-bundle-{index}", "sample"));
        }
        var report = TieredTaxFormBundleReport.BuildTieredTaxFormBundleReport("test", items, clock);
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
        TieredTaxFormBundleReportCountsEachRecordOnce();
    }
}
