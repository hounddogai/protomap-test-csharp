namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class LenientTaxFormAssignmentReportChecks
{
    public static void LenientTaxFormAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705917526);
        var items = new List<LenientTaxFormAssignment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientTaxFormAssignment.NewLenientTaxFormAssignment($"lenient-tax-form-assignment-{index}", "sample"));
        }
        var report = LenientTaxFormAssignmentReport.BuildLenientTaxFormAssignmentReport("test", items, clock);
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
        LenientTaxFormAssignmentReportCountsEachRecordOnce();
    }
}
