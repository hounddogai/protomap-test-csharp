namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingTrainingDueLedgerChecks
{
    public static void PendingTrainingDueLedgerStartsInCreated()
    {
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("code-162", "name-36");
        Check.Equal(item.Status, PendingTrainingDueLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTrainingDueLedgerMovesToRetired()
    {
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("code-162", "name-36");
        try
        {
            item.MoveTo(PendingTrainingDueLedgerStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTrainingDueLedgerStatus.Retired, "status");
    }

    public static void PendingTrainingDueLedgerRejectsMovesFromRetired()
    {
        var item = PendingTrainingDueLedger.NewPendingTrainingDueLedger("code-162", "name-36");
        item.Status = PendingTrainingDueLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingTrainingDueLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingTrainingDueLedgerStartsInCreated();
        PendingTrainingDueLedgerMovesToRetired();
        PendingTrainingDueLedgerRejectsMovesFromRetired();
    }
}
