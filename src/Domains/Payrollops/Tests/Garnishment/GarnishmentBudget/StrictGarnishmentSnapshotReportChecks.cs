namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class StrictGarnishmentSnapshotReportChecks
{
    public static void StrictGarnishmentSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703184410);
        var items = new List<StrictGarnishmentSnapshot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StrictGarnishmentSnapshot.NewStrictGarnishmentSnapshot($"strict-garnishment-snapshot-{index}", "sample"));
        }
        var report = StrictGarnishmentSnapshotReport.BuildStrictGarnishmentSnapshotReport("test", items, clock);
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
        StrictGarnishmentSnapshotReportCountsEachRecordOnce();
    }
}
