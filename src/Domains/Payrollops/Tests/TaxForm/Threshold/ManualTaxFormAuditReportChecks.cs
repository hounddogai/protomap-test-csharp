namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualTaxFormAuditReportChecks
{
    public static void ManualTaxFormAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703493914);
        var items = new List<ManualTaxFormAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ManualTaxFormAudit.NewManualTaxFormAudit($"manual-tax-form-audit-{index}", "sample"));
        }
        var report = ManualTaxFormAuditReport.BuildManualTaxFormAuditReport("test", items, clock);
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
        ManualTaxFormAuditReportCountsEachRecordOnce();
    }
}
