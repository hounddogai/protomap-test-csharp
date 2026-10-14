namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TaxFormReservationReportChecks
{
    public static void TaxFormReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706086859);
        var items = new List<TaxFormReservation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TaxFormReservation.NewTaxFormReservation($"tax-form-reservation-{index}", "sample"));
        }
        var report = TaxFormReservationReport.BuildTaxFormReservationReport("test", items, clock);
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
        TaxFormReservationReportCountsEachRecordOnce();
    }
}
