namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RefreshAuditReportChecks
{
    public static void RefreshAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701781593);
        var items = new List<RefreshAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RefreshAudit.NewRefreshAudit($"refresh-audit-{index}", "sample"));
        }
        var report = RefreshAuditReport.BuildRefreshAuditReport("test", items, clock);
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
        RefreshAuditReportCountsEachRecordOnce();
    }
}
