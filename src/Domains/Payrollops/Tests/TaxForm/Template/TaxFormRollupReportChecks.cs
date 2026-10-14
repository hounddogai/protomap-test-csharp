namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormRollupReportChecks
{
    public static void TaxFormRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705178153);
        var items = new List<TaxFormRollup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TaxFormRollup.NewTaxFormRollup($"tax-form-rollup-{index}", "sample"));
        }
        var report = TaxFormRollupReport.BuildTaxFormRollupReport("test", items, clock);
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
        TaxFormRollupReportCountsEachRecordOnce();
    }
}
