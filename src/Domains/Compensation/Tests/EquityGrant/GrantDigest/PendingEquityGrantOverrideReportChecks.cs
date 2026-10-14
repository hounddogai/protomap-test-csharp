namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PendingEquityGrantOverrideReportChecks
{
    public static void PendingEquityGrantOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707417230);
        var items = new List<PendingEquityGrantOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PendingEquityGrantOverride.NewPendingEquityGrantOverride($"pending-equity-grant-override-{index}", "sample"));
        }
        var report = PendingEquityGrantOverrideReport.BuildPendingEquityGrantOverrideReport("test", items, clock);
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
        PendingEquityGrantOverrideReportCountsEachRecordOnce();
    }
}
