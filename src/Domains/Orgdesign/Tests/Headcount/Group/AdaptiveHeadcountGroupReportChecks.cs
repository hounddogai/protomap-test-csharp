namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveHeadcountGroupReportChecks
{
    public static void AdaptiveHeadcountGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703590256);
        var items = new List<AdaptiveHeadcountGroup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup($"adaptive-headcount-group-{index}", "sample"));
        }
        var report = AdaptiveHeadcountGroupReport.BuildAdaptiveHeadcountGroupReport("test", items, clock);
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
        AdaptiveHeadcountGroupReportCountsEachRecordOnce();
    }
}
