namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class CappedTaxFormAssignmentReportChecks
{
    public static void CappedTaxFormAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703065625);
        var items = new List<CappedTaxFormAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedTaxFormAssignment.NewCappedTaxFormAssignment($"capped-tax-form-assignment-{index}", "sample"));
        }
        var report = CappedTaxFormAssignmentReport.BuildCappedTaxFormAssignmentReport("test", items, clock);
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
        CappedTaxFormAssignmentReportCountsEachRecordOnce();
    }
}
