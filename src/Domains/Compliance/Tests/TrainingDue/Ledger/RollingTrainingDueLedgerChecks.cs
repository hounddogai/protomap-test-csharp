namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingTrainingDueLedgerChecks
{
    public static void RollingTrainingDueLedgerStartsInNew()
    {
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("code-937", "label-103");
        Check.Equal(item.Status, RollingTrainingDueLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingTrainingDueLedgerMovesToQueued()
    {
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("code-937", "label-103");
        try
        {
            item.MoveTo(RollingTrainingDueLedgerStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingTrainingDueLedgerStatus.Queued, "status");
    }

    public static void RollingTrainingDueLedgerRejectsMovesFromFailed()
    {
        var item = RollingTrainingDueLedger.NewRollingTrainingDueLedger("code-937", "label-103");
        item.Status = RollingTrainingDueLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingTrainingDueLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingTrainingDueLedgerStartsInNew();
        RollingTrainingDueLedgerMovesToQueued();
        RollingTrainingDueLedgerRejectsMovesFromFailed();
    }
}
