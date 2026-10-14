namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAdjustmentChecks
{
    public static void RetroPayAdjustmentStartsInProposed()
    {
        var item = RetroPayAdjustment.NewRetroPayAdjustment("code-775", "label-65");
        Check.Equal(item.Status, RetroPayAdjustmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayAdjustmentMovesToApproved()
    {
        var item = RetroPayAdjustment.NewRetroPayAdjustment("code-775", "label-65");
        try
        {
            item.MoveTo(RetroPayAdjustmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayAdjustmentStatus.Approved, "status");
    }

    public static void RetroPayAdjustmentRejectsMovesFromWithdrawn()
    {
        var item = RetroPayAdjustment.NewRetroPayAdjustment("code-775", "label-65");
        item.Status = RetroPayAdjustmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RetroPayAdjustmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RetroPayAdjustmentStartsInProposed();
        RetroPayAdjustmentMovesToApproved();
        RetroPayAdjustmentRejectsMovesFromWithdrawn();
    }
}
