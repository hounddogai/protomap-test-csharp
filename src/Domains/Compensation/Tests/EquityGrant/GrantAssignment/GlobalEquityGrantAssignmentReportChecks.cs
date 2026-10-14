namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class GlobalEquityGrantAssignmentReportChecks
{
    public static void GlobalEquityGrantAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703342524);
        var items = new List<GlobalEquityGrantAssignment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment($"global-equity-grant-assignment-{index}", "sample"));
        }
        var report = GlobalEquityGrantAssignmentReport.BuildGlobalEquityGrantAssignmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        GlobalEquityGrantAssignmentReportCountsEachRecordOnce();
    }
}
