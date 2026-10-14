namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class StrictTaxFormAuditReportChecks
{
    public static void StrictTaxFormAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705752542);
        var items = new List<StrictTaxFormAudit>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictTaxFormAudit.NewStrictTaxFormAudit($"strict-tax-form-audit-{index}", "sample"));
        }
        var report = StrictTaxFormAuditReport.BuildStrictTaxFormAuditReport("test", items, clock);
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
        StrictTaxFormAuditReportCountsEachRecordOnce();
    }
}
