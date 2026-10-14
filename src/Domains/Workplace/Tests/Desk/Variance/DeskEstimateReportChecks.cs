namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class DeskEstimateReportChecks
{
    public static void DeskEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703108678);
        var items = new List<DeskEstimate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DeskEstimate.NewDeskEstimate($"desk-estimate-{index}", "sample"));
        }
        var report = DeskEstimateReport.BuildDeskEstimateReport("test", items, clock);
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
        DeskEstimateReportCountsEachRecordOnce();
    }
}
