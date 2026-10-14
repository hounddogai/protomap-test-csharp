namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayPlanChecks
{
    public static void RetroPayPlanStartsInIdle()
    {
        var item = RetroPayPlan.NewRetroPayPlan("code-539", "name-23");
        Check.Equal(item.Status, RetroPayPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayPlanMovesToWarming()
    {
        var item = RetroPayPlan.NewRetroPayPlan("code-539", "name-23");
        try
        {
            item.MoveTo(RetroPayPlanStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayPlanStatus.Warming, "status");
    }

    public static void RetroPayPlanRejectsMovesFromStopped()
    {
        var item = RetroPayPlan.NewRetroPayPlan("code-539", "name-23");
        item.Status = RetroPayPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(RetroPayPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RetroPayPlanStartsInIdle();
        RetroPayPlanMovesToWarming();
        RetroPayPlanRejectsMovesFromStopped();
    }
}
