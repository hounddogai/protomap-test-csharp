namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshAssignmentReportChecks
{
    public static void RefreshAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705650492);
        var items = new List<RefreshAssignment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RefreshAssignment.NewRefreshAssignment($"refresh-assignment-{index}", "sample"));
        }
        var report = RefreshAssignmentReport.BuildRefreshAssignmentReport("test", items, clock);
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
        RefreshAssignmentReportCountsEachRecordOnce();
    }
}
