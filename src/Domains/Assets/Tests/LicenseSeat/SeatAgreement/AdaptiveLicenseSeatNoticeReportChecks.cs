namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatNoticeReportChecks
{
    public static void AdaptiveLicenseSeatNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704791564);
        var items = new List<AdaptiveLicenseSeatNotice>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice($"adaptive-license-seat-notice-{index}", "sample"));
        }
        var report = AdaptiveLicenseSeatNoticeReport.BuildAdaptiveLicenseSeatNoticeReport("test", items, clock);
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
        AdaptiveLicenseSeatNoticeReportCountsEachRecordOnce();
    }
}
