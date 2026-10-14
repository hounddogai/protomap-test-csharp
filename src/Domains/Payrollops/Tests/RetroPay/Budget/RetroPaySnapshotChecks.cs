namespace Acme.Workforce.Payrollops.Tests.RetroPay.Budget;

using Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RetroPaySnapshotChecks
{
    public static void RetroPaySnapshotStartsInNew()
    {
        var item = RetroPaySnapshot.NewRetroPaySnapshot("code-770", "label-476");
        Check.Equal(item.Status, RetroPaySnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPaySnapshotMovesToDone()
    {
        var item = RetroPaySnapshot.NewRetroPaySnapshot("code-770", "label-476");
        try
        {
            item.MoveTo(RetroPaySnapshotStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPaySnapshotStatus.Done, "status");
    }

    public static void RetroPaySnapshotRejectsMovesFromFailed()
    {
        var item = RetroPaySnapshot.NewRetroPaySnapshot("code-770", "label-476");
        item.Status = RetroPaySnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(RetroPaySnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        RetroPaySnapshotStartsInNew();
        RetroPaySnapshotMovesToDone();
        RetroPaySnapshotRejectsMovesFromFailed();
    }
}
