namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimarySpanEstimateReportChecks
{
    public static void PrimarySpanEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701521741);
        var items = new List<PrimarySpanEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimarySpanEstimate.NewPrimarySpanEstimate($"primary-span-estimate-{index}", "sample"));
        }
        var report = PrimarySpanEstimateReport.BuildPrimarySpanEstimateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PrimarySpanEstimateReportCountsEachRecordOnce();
    }
}
