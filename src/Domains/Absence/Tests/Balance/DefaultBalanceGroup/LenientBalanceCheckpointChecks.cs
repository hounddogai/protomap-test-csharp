namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceCheckpointChecks
{
    public static void LenientBalanceCheckpointStartsInProposed()
    {
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("code-728", "title-794");
        Check.Equal(item.Status, LenientBalanceCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBalanceCheckpointMovesToApproved()
    {
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("code-728", "title-794");
        try
        {
            item.MoveTo(LenientBalanceCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBalanceCheckpointStatus.Approved, "status");
    }

    public static void LenientBalanceCheckpointRejectsMovesFromWithdrawn()
    {
        var item = LenientBalanceCheckpoint.NewLenientBalanceCheckpoint("code-728", "title-794");
        item.Status = LenientBalanceCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientBalanceCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientBalanceCheckpointStartsInProposed();
        LenientBalanceCheckpointMovesToApproved();
        LenientBalanceCheckpointRejectsMovesFromWithdrawn();
    }
}
