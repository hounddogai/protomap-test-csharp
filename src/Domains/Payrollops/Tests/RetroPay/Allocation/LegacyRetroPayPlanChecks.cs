namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LegacyRetroPayPlanChecks
{
    public static void LegacyRetroPayPlanStartsInIdle()
    {
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("code-11", "name-743");
        Check.Equal(item.Status, LegacyRetroPayPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRetroPayPlanMovesToReady()
    {
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("code-11", "name-743");
        try
        {
            item.MoveTo(LegacyRetroPayPlanStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRetroPayPlanStatus.Ready, "status");
    }

    public static void LegacyRetroPayPlanRejectsMovesFromStopped()
    {
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("code-11", "name-743");
        item.Status = LegacyRetroPayPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyRetroPayPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyRetroPayPlanStartsInIdle();
        LegacyRetroPayPlanMovesToReady();
        LegacyRetroPayPlanRejectsMovesFromStopped();
    }
}
