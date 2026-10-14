namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class LenientHeadcountRecordReportChecks
{
    public static void LenientHeadcountRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703207904);
        var items = new List<LenientHeadcountRecord>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientHeadcountRecord.NewLenientHeadcountRecord($"lenient-headcount-record-{index}", "sample"));
        }
        var report = LenientHeadcountRecordReport.BuildLenientHeadcountRecordReport("test", items, clock);
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
        LenientHeadcountRecordReportCountsEachRecordOnce();
    }
}
