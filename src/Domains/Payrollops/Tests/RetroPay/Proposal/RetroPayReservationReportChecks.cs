namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayReservationReportChecks
{
    public static void RetroPayReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703823167);
        var items = new List<RetroPayReservation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RetroPayReservation.NewRetroPayReservation($"retro-pay-reservation-{index}", "sample"));
        }
        var report = RetroPayReservationReport.BuildRetroPayReservationReport("test", items, clock);
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
        RetroPayReservationReportCountsEachRecordOnce();
    }
}
