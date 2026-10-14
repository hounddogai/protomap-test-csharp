namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyPayRunRequestReportChecks
{
    public static void LegacyPayRunRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709940151);
        var items = new List<LegacyPayRunRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LegacyPayRunRequest.NewLegacyPayRunRequest($"legacy-pay-run-request-{index}", "sample"));
        }
        var report = LegacyPayRunRequestReport.BuildLegacyPayRunRequestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LegacyPayRunRequestReportCountsEachRecordOnce();
    }
}
