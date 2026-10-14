namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingTrainingDueLedgerReportChecks
{
    public static void RollingTrainingDueLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709746353);
        var items = new List<RollingTrainingDueLedger>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RollingTrainingDueLedger.NewRollingTrainingDueLedger($"rolling-training-due-ledger-{index}", "sample"));
        }
        var report = RollingTrainingDueLedgerReport.BuildRollingTrainingDueLedgerReport("test", items, clock);
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
        RollingTrainingDueLedgerReportCountsEachRecordOnce();
    }
}
