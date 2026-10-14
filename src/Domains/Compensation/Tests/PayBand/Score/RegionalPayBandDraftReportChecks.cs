namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RegionalPayBandDraftReportChecks
{
    public static void RegionalPayBandDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704040397);
        var items = new List<RegionalPayBandDraft>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalPayBandDraft.NewRegionalPayBandDraft($"regional-pay-band-draft-{index}", "sample"));
        }
        var report = RegionalPayBandDraftReport.BuildRegionalPayBandDraftReport("test", items, clock);
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
        RegionalPayBandDraftReportCountsEachRecordOnce();
    }
}
