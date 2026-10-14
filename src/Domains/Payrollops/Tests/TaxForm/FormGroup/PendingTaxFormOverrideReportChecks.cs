namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class PendingTaxFormOverrideReportChecks
{
    public static void PendingTaxFormOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702331164);
        var items = new List<PendingTaxFormOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PendingTaxFormOverride.NewPendingTaxFormOverride($"pending-tax-form-override-{index}", "sample"));
        }
        var report = PendingTaxFormOverrideReport.BuildPendingTaxFormOverrideReport("test", items, clock);
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
        PendingTaxFormOverrideReportCountsEachRecordOnce();
    }
}
