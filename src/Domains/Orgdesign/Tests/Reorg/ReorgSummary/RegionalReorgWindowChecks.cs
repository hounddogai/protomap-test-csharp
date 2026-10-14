namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class RegionalReorgWindowChecks
{
    public static void RegionalReorgWindowStartsInRequested()
    {
        var item = RegionalReorgWindow.NewRegionalReorgWindow("code-644", "label-979");
        Check.Equal(item.Status, RegionalReorgWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalReorgWindowMovesToRevoked()
    {
        var item = RegionalReorgWindow.NewRegionalReorgWindow("code-644", "label-979");
        try
        {
            item.MoveTo(RegionalReorgWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalReorgWindowStatus.Revoked, "status");
    }

    public static void RegionalReorgWindowRejectsMovesFromExpired()
    {
        var item = RegionalReorgWindow.NewRegionalReorgWindow("code-644", "label-979");
        item.Status = RegionalReorgWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalReorgWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalReorgWindowStartsInRequested();
        RegionalReorgWindowMovesToRevoked();
        RegionalReorgWindowRejectsMovesFromExpired();
    }
}
