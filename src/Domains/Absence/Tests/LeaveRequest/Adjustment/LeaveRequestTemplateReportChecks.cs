namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestTemplateReportChecks
{
    public static void LeaveRequestTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703227663);
        var items = new List<LeaveRequestTemplate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LeaveRequestTemplate.NewLeaveRequestTemplate($"leave-request-template-{index}", "sample"));
        }
        var report = LeaveRequestTemplateReport.BuildLeaveRequestTemplateReport("test", items, clock);
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
        LeaveRequestTemplateReportCountsEachRecordOnce();
    }
}
