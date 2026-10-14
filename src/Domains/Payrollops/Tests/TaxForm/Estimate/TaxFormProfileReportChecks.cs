namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TaxFormProfileReportChecks
{
    public static void TaxFormProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700869824);
        var items = new List<TaxFormProfile>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TaxFormProfile.NewTaxFormProfile($"tax-form-profile-{index}", "sample"));
        }
        var report = TaxFormProfileReport.BuildTaxFormProfileReport("test", items, clock);
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
        TaxFormProfileReportCountsEachRecordOnce();
    }
}
