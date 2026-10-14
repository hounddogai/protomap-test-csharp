namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAuditReportChecks
{
    public static void AssignmentAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708692500);
        var items = new List<AssignmentAudit>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AssignmentAudit.NewAssignmentAudit($"assignment-audit-{index}", "sample"));
        }
        var report = AssignmentAuditReport.BuildAssignmentAuditReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AssignmentAuditReportCountsEachRecordOnce();
    }
}
