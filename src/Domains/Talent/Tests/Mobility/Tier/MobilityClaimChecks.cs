namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityClaimChecks
{
    public static void MobilityClaimStartsInOpen()
    {
        var item = MobilityClaim.NewMobilityClaim("code-990", "name-964");
        Check.Equal(item.Status, MobilityClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityClaimMovesToResolved()
    {
        var item = MobilityClaim.NewMobilityClaim("code-990", "name-964");
        try
        {
            item.MoveTo(MobilityClaimStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityClaimStatus.Resolved, "status");
    }

    public static void MobilityClaimRejectsMovesFromArchived()
    {
        var item = MobilityClaim.NewMobilityClaim("code-990", "name-964");
        item.Status = MobilityClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(MobilityClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        MobilityClaimStartsInOpen();
        MobilityClaimMovesToResolved();
        MobilityClaimRejectsMovesFromArchived();
    }
}
