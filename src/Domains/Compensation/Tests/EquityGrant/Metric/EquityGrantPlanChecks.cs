namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantPlanChecks
{
    public static void EquityGrantPlanStartsInIdle()
    {
        var item = EquityGrantPlan.NewEquityGrantPlan("code-128", "title-633");
        Check.Equal(item.Status, EquityGrantPlanStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantPlanMovesToStopped()
    {
        var item = EquityGrantPlan.NewEquityGrantPlan("code-128", "title-633");
        try
        {
            item.MoveTo(EquityGrantPlanStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantPlanStatus.Stopped, "status");
    }

    public static void EquityGrantPlanRejectsMovesFromStopped()
    {
        var item = EquityGrantPlan.NewEquityGrantPlan("code-128", "title-633");
        item.Status = EquityGrantPlanStatus.Stopped;
        Check.Fails(() => item.MoveTo(EquityGrantPlanStatus.Idle), "move");
    }

    public static void RunAll()
    {
        EquityGrantPlanStartsInIdle();
        EquityGrantPlanMovesToStopped();
        EquityGrantPlanRejectsMovesFromStopped();
    }
}
