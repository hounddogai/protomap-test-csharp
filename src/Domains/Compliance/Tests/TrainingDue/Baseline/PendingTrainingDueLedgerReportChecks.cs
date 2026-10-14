namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingTrainingDueLedgerReportChecks
{
    public static void PendingTrainingDueLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706778504);
        var items = new List<PendingTrainingDueLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingTrainingDueLedger.NewPendingTrainingDueLedger($"pending-training-due-ledger-{index}", "sample"));
        }
        var report = PendingTrainingDueLedgerReport.BuildPendingTrainingDueLedgerReport("test", items, clock);
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
        PendingTrainingDueLedgerReportCountsEachRecordOnce();
    }
}
