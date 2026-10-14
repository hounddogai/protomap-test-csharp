namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultMobilityClaimChecks
{
    public static void DefaultMobilityClaimStartsInIdle()
    {
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("code-628", "name-29");
        Check.Equal(item.Status, DefaultMobilityClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultMobilityClaimMovesToReady()
    {
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("code-628", "name-29");
        try
        {
            item.MoveTo(DefaultMobilityClaimStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultMobilityClaimStatus.Ready, "status");
    }

    public static void DefaultMobilityClaimRejectsMovesFromStopped()
    {
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("code-628", "name-29");
        item.Status = DefaultMobilityClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultMobilityClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultMobilityClaimStartsInIdle();
        DefaultMobilityClaimMovesToReady();
        DefaultMobilityClaimRejectsMovesFromStopped();
    }
}
