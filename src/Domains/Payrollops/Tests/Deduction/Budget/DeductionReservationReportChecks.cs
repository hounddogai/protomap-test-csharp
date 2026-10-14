namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionReservationReportChecks
{
    public static void DeductionReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707355283);
        var items = new List<DeductionReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeductionReservation.NewDeductionReservation($"deduction-reservation-{index}", "sample"));
        }
        var report = DeductionReservationReport.BuildDeductionReservationReport("test", items, clock);
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
        DeductionReservationReportCountsEachRecordOnce();
    }
}
