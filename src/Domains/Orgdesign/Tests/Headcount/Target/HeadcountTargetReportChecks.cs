namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class HeadcountTargetReportChecks
{
    public static void HeadcountTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708996419);
        var items = new List<HeadcountTarget>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HeadcountTarget.NewHeadcountTarget($"headcount-target-{index}", "sample"));
        }
        var report = HeadcountTargetReport.BuildHeadcountTargetReport("test", items, clock);
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
        HeadcountTargetReportCountsEachRecordOnce();
    }
}
