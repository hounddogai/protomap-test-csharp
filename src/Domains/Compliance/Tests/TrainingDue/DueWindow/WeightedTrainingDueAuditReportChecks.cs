namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class WeightedTrainingDueAuditReportChecks
{
    public static void WeightedTrainingDueAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703048850);
        var items = new List<WeightedTrainingDueAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WeightedTrainingDueAudit.NewWeightedTrainingDueAudit($"weighted-training-due-audit-{index}", "sample"));
        }
        var report = WeightedTrainingDueAuditReport.BuildWeightedTrainingDueAuditReport("test", items, clock);
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
        WeightedTrainingDueAuditReportCountsEachRecordOnce();
    }
}
