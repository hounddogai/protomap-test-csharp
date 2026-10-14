namespace Acme.Workforce.Orgdesign.Tests.Headcount.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class HeadcountEstimateReportChecks
{
    public static void HeadcountEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702158143);
        var items = new List<HeadcountEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HeadcountEstimate.NewHeadcountEstimate($"headcount-estimate-{index}", "sample"));
        }
        var report = HeadcountEstimateReport.BuildHeadcountEstimateReport("test", items, clock);
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
        HeadcountEstimateReportCountsEachRecordOnce();
    }
}
