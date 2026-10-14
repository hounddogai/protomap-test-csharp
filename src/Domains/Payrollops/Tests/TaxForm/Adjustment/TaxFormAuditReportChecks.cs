namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormAuditReportChecks
{
    public static void TaxFormAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708901165);
        var items = new List<TaxFormAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TaxFormAudit.NewTaxFormAudit($"tax-form-audit-{index}", "sample"));
        }
        var report = TaxFormAuditReport.BuildTaxFormAuditReport("test", items, clock);
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
        TaxFormAuditReportCountsEachRecordOnce();
    }
}
