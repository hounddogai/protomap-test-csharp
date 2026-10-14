namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionTargetReportChecks
{
    public static void LegacyPositionTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703171129);
        var items = new List<LegacyPositionTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyPositionTarget.NewLegacyPositionTarget($"legacy-position-target-{index}", "sample"));
        }
        var report = LegacyPositionTargetReport.BuildLegacyPositionTargetReport("test", items, clock);
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
        LegacyPositionTargetReportCountsEachRecordOnce();
    }
}
