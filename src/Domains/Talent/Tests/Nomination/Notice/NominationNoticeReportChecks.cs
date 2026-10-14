namespace Acme.Workforce.Talent.Tests.Nomination.Notice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class NominationNoticeReportChecks
{
    public static void NominationNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709704954);
        var items = new List<NominationNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(NominationNotice.NewNominationNotice($"nomination-notice-{index}", "sample"));
        }
        var report = NominationNoticeReport.BuildNominationNoticeReport("test", items, clock);
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
        NominationNoticeReportCountsEachRecordOnce();
    }
}
