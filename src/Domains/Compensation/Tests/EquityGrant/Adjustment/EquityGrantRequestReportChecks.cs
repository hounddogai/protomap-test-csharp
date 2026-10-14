namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantRequestReportChecks
{
    public static void EquityGrantRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709927168);
        var items = new List<EquityGrantRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EquityGrantRequest.NewEquityGrantRequest($"equity-grant-request-{index}", "sample"));
        }
        var report = EquityGrantRequestReport.BuildEquityGrantRequestReport("test", items, clock);
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
        EquityGrantRequestReportCountsEachRecordOnce();
    }
}
