namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormOverrideReportChecks
{
    public static void TaxFormOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707990518);
        var items = new List<TaxFormOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TaxFormOverride.NewTaxFormOverride($"tax-form-override-{index}", "sample"));
        }
        var report = TaxFormOverrideReport.BuildTaxFormOverrideReport("test", items, clock);
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
        TaxFormOverrideReportCountsEachRecordOnce();
    }
}
