namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueAuditReportChecks
{
    public static void TrainingDueAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704616186);
        var items = new List<TrainingDueAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TrainingDueAudit.NewTrainingDueAudit($"training-due-audit-{index}", "sample"));
        }
        var report = TrainingDueAuditReport.BuildTrainingDueAuditReport("test", items, clock);
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
        TrainingDueAuditReportCountsEachRecordOnce();
    }
}
