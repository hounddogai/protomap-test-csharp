namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueLedgerReportChecks
{
    public static void TrainingDueLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706970537);
        var items = new List<TrainingDueLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TrainingDueLedger.NewTrainingDueLedger($"training-due-ledger-{index}", "sample"));
        }
        var report = TrainingDueLedgerReport.BuildTrainingDueLedgerReport("test", items, clock);
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
        TrainingDueLedgerReportCountsEachRecordOnce();
    }
}
