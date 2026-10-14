namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackRefreshPolicyReportChecks
{
    public static void FallbackRefreshPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703332181);
        var items = new List<FallbackRefreshPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FallbackRefreshPolicy.NewFallbackRefreshPolicy($"fallback-refresh-policy-{index}", "sample"));
        }
        var report = FallbackRefreshPolicyReport.BuildFallbackRefreshPolicyReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FallbackRefreshPolicyReportCountsEachRecordOnce();
    }
}
