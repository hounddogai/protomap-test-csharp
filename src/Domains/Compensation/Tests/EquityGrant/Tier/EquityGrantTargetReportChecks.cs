namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantTargetReportChecks
{
    public static void EquityGrantTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703886786);
        var items = new List<EquityGrantTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(EquityGrantTarget.NewEquityGrantTarget($"equity-grant-target-{index}", "sample"));
        }
        var report = EquityGrantTargetReport.BuildEquityGrantTargetReport("test", items, clock);
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
        EquityGrantTargetReportCountsEachRecordOnce();
    }
}
