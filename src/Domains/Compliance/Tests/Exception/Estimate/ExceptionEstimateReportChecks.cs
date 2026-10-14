namespace Acme.Workforce.Compliance.Tests.Exception.Estimate;

using Acme.Workforce.Compliance.Exception.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ExceptionEstimateReportChecks
{
    public static void ExceptionEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706728976);
        var items = new List<ExceptionEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ExceptionEstimate.NewExceptionEstimate($"exception-estimate-{index}", "sample"));
        }
        var report = ExceptionEstimateReport.BuildExceptionEstimateReport("test", items, clock);
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
        ExceptionEstimateReportCountsEachRecordOnce();
    }
}
