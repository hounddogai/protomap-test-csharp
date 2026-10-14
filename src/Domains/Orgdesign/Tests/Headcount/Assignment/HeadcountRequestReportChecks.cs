namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountRequestReportChecks
{
    public static void HeadcountRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705722701);
        var items = new List<HeadcountRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HeadcountRequest.NewHeadcountRequest($"headcount-request-{index}", "sample"));
        }
        var report = HeadcountRequestReport.BuildHeadcountRequestReport("test", items, clock);
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
        HeadcountRequestReportCountsEachRecordOnce();
    }
}
