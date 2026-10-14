namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredEquityGrantWindowChecks
{
    public static void TieredEquityGrantWindowStartsInRequested()
    {
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("code-401", "name-199");
        Check.Equal(item.Status, TieredEquityGrantWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredEquityGrantWindowMovesToRevoked()
    {
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("code-401", "name-199");
        try
        {
            item.MoveTo(TieredEquityGrantWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredEquityGrantWindowStatus.Revoked, "status");
    }

    public static void TieredEquityGrantWindowRejectsMovesFromExpired()
    {
        var item = TieredEquityGrantWindow.NewTieredEquityGrantWindow("code-401", "name-199");
        item.Status = TieredEquityGrantWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredEquityGrantWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredEquityGrantWindowStartsInRequested();
        TieredEquityGrantWindowMovesToRevoked();
        TieredEquityGrantWindowRejectsMovesFromExpired();
    }
}
