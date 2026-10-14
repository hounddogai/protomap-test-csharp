namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentLedgerChecks
{
    public static void AssignmentLedgerStartsInNew()
    {
        var item = AssignmentLedger.NewAssignmentLedger("code-998", "label-957");
        Check.Equal(item.Status, AssignmentLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentLedgerMovesToFailed()
    {
        var item = AssignmentLedger.NewAssignmentLedger("code-998", "label-957");
        try
        {
            item.MoveTo(AssignmentLedgerStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentLedgerStatus.Failed, "status");
    }

    public static void AssignmentLedgerRejectsMovesFromFailed()
    {
        var item = AssignmentLedger.NewAssignmentLedger("code-998", "label-957");
        item.Status = AssignmentLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(AssignmentLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        AssignmentLedgerStartsInNew();
        AssignmentLedgerMovesToFailed();
        AssignmentLedgerRejectsMovesFromFailed();
    }
}
