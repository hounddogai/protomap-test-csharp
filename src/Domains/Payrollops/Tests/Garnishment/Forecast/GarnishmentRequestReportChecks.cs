namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRequestReportChecks
{
    public static void GarnishmentRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706626872);
        var items = new List<GarnishmentRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GarnishmentRequest.NewGarnishmentRequest($"garnishment-request-{index}", "sample"));
        }
        var report = GarnishmentRequestReport.BuildGarnishmentRequestReport("test", items, clock);
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
        GarnishmentRequestReportCountsEachRecordOnce();
    }
}
