namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayCycleChecks
{
    public static void RetroPayCycleStartsInDraft()
    {
        var item = RetroPayCycle.NewRetroPayCycle("code-485", "name-145");
        Check.Equal(item.Status, RetroPayCycleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayCycleMovesToActive()
    {
        var item = RetroPayCycle.NewRetroPayCycle("code-485", "name-145");
        try
        {
            item.MoveTo(RetroPayCycleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayCycleStatus.Active, "status");
    }

    public static void RetroPayCycleRejectsMovesFromClosed()
    {
        var item = RetroPayCycle.NewRetroPayCycle("code-485", "name-145");
        item.Status = RetroPayCycleStatus.Closed;
        Check.Fails(() => item.MoveTo(RetroPayCycleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RetroPayCycleStartsInDraft();
        RetroPayCycleMovesToActive();
        RetroPayCycleRejectsMovesFromClosed();
    }
}
