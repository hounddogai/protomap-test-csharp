namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CappedBookingAgreementReportChecks
{
    public static void CappedBookingAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707185123);
        var items = new List<CappedBookingAgreement>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedBookingAgreement.NewCappedBookingAgreement($"capped-booking-agreement-{index}", "sample"));
        }
        var report = CappedBookingAgreementReport.BuildCappedBookingAgreementReport("test", items, clock);
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
        CappedBookingAgreementReportCountsEachRecordOnce();
    }
}
