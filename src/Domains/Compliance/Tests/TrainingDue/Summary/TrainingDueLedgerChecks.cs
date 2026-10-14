namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueLedgerChecks
{
    public static void TrainingDueLedgerStartsInOpen()
    {
        var item = TrainingDueLedger.NewTrainingDueLedger("code-371", "name-830");
        Check.Equal(item.Status, TrainingDueLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueLedgerMovesToReopened()
    {
        var item = TrainingDueLedger.NewTrainingDueLedger("code-371", "name-830");
        try
        {
            item.MoveTo(TrainingDueLedgerStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueLedgerStatus.Reopened, "status");
    }

    public static void TrainingDueLedgerRejectsMovesFromArchived()
    {
        var item = TrainingDueLedger.NewTrainingDueLedger("code-371", "name-830");
        item.Status = TrainingDueLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(TrainingDueLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        TrainingDueLedgerStartsInOpen();
        TrainingDueLedgerMovesToReopened();
        TrainingDueLedgerRejectsMovesFromArchived();
    }
}
