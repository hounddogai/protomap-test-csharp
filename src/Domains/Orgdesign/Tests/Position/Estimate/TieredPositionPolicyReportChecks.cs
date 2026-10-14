namespace Acme.Workforce.Orgdesign.Tests.Position.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredPositionPolicyReportChecks
{
    public static void TieredPositionPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700021252);
        var items = new List<TieredPositionPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredPositionPolicy.NewTieredPositionPolicy($"tiered-position-policy-{index}", "sample"));
        }
        var report = TieredPositionPolicyReport.BuildTieredPositionPolicyReport("test", items, clock);
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
        TieredPositionPolicyReportCountsEachRecordOnce();
    }
}
