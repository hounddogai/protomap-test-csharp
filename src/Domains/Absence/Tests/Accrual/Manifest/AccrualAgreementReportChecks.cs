namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualAgreementReportChecks
{
    public static void AccrualAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700546952);
        var items = new List<AccrualAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AccrualAgreement.NewAccrualAgreement($"accrual-agreement-{index}", "sample"));
        }
        var report = AccrualAgreementReport.BuildAccrualAgreementReport("test", items, clock);
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
        AccrualAgreementReportCountsEachRecordOnce();
    }
}
