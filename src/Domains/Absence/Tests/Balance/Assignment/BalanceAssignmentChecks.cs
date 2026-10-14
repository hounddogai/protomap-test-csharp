namespace Acme.Workforce.Absence.Tests.Balance.Assignment;

using Acme.Workforce.Absence.Balance.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BalanceAssignmentChecks
{
    public static void BalanceAssignmentStartsInNew()
    {
        var item = BalanceAssignment.NewBalanceAssignment("code-637", "title-233");
        Check.Equal(item.Status, BalanceAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceAssignmentMovesToRunning()
    {
        var item = BalanceAssignment.NewBalanceAssignment("code-637", "title-233");
        try
        {
            item.MoveTo(BalanceAssignmentStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceAssignmentStatus.Running, "status");
    }

    public static void BalanceAssignmentRejectsMovesFromFailed()
    {
        var item = BalanceAssignment.NewBalanceAssignment("code-637", "title-233");
        item.Status = BalanceAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(BalanceAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        BalanceAssignmentStartsInNew();
        BalanceAssignmentMovesToRunning();
        BalanceAssignmentRejectsMovesFromFailed();
    }
}
