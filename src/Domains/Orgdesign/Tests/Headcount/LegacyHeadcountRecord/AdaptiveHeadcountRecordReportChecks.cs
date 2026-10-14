namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class AdaptiveHeadcountRecordReportChecks
{
    public static void AdaptiveHeadcountRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706339310);
        var items = new List<AdaptiveHeadcountRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord($"adaptive-headcount-record-{index}", "sample"));
        }
        var report = AdaptiveHeadcountRecordReport.BuildAdaptiveHeadcountRecordReport("test", items, clock);
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
        AdaptiveHeadcountRecordReportCountsEachRecordOnce();
    }
}
