namespace Acme.Workforce.Payrollops.Tests.RetroPay.Policy;

using Acme.Workforce.Payrollops.RetroPay.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RetroPayPolicyChecks
{
    public static void RetroPayPolicyStartsInPlanned()
    {
        var item = RetroPayPolicy.NewRetroPayPolicy("code-417", "name-898");
        Check.Equal(item.Status, RetroPayPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayPolicyMovesToCompleted()
    {
        var item = RetroPayPolicy.NewRetroPayPolicy("code-417", "name-898");
        try
        {
            item.MoveTo(RetroPayPolicyStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayPolicyStatus.Completed, "status");
    }

    public static void RetroPayPolicyRejectsMovesFromCancelled()
    {
        var item = RetroPayPolicy.NewRetroPayPolicy("code-417", "name-898");
        item.Status = RetroPayPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RetroPayPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RetroPayPolicyStartsInPlanned();
        RetroPayPolicyMovesToCompleted();
        RetroPayPolicyRejectsMovesFromCancelled();
    }
}
