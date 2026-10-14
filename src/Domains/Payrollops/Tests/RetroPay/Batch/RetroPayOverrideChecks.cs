namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RetroPayOverrideChecks
{
    public static void RetroPayOverrideStartsInCreated()
    {
        var item = RetroPayOverride.NewRetroPayOverride("code-279", "label-858");
        Check.Equal(item.Status, RetroPayOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayOverrideMovesToRetired()
    {
        var item = RetroPayOverride.NewRetroPayOverride("code-279", "label-858");
        try
        {
            item.MoveTo(RetroPayOverrideStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayOverrideStatus.Retired, "status");
    }

    public static void RetroPayOverrideRejectsMovesFromRetired()
    {
        var item = RetroPayOverride.NewRetroPayOverride("code-279", "label-858");
        item.Status = RetroPayOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(RetroPayOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        RetroPayOverrideStartsInCreated();
        RetroPayOverrideMovesToRetired();
        RetroPayOverrideRejectsMovesFromRetired();
    }
}
