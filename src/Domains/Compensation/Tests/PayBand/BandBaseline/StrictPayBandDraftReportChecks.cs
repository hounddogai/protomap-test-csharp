namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class StrictPayBandDraftReportChecks
{
    public static void StrictPayBandDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707576870);
        var items = new List<StrictPayBandDraft>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictPayBandDraft.NewStrictPayBandDraft($"strict-pay-band-draft-{index}", "sample"));
        }
        var report = StrictPayBandDraftReport.BuildStrictPayBandDraftReport("test", items, clock);
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
        StrictPayBandDraftReportCountsEachRecordOnce();
    }
}
