namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceForecastChecks
{
    public static void BalanceForecastStartsInProposed()
    {
        var item = BalanceForecast.NewBalanceForecast("code-218", "name-321");
        Check.Equal(item.Status, BalanceForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceForecastMovesToApproved()
    {
        var item = BalanceForecast.NewBalanceForecast("code-218", "name-321");
        try
        {
            item.MoveTo(BalanceForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceForecastStatus.Approved, "status");
    }

    public static void BalanceForecastRejectsMovesFromWithdrawn()
    {
        var item = BalanceForecast.NewBalanceForecast("code-218", "name-321");
        item.Status = BalanceForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BalanceForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BalanceForecastStartsInProposed();
        BalanceForecastMovesToApproved();
        BalanceForecastRejectsMovesFromWithdrawn();
    }
}
