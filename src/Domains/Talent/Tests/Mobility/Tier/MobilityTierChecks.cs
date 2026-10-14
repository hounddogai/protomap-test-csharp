namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityTierChecks
{
    public static void MobilityTierStartsInOpen()
    {
        var item = MobilityTier.NewMobilityTier("code-951", "label-341");
        Check.Equal(item.Status, MobilityTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityTierMovesToInReview()
    {
        var item = MobilityTier.NewMobilityTier("code-951", "label-341");
        try
        {
            item.MoveTo(MobilityTierStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityTierStatus.InReview, "status");
    }

    public static void MobilityTierRejectsMovesFromArchived()
    {
        var item = MobilityTier.NewMobilityTier("code-951", "label-341");
        item.Status = MobilityTierStatus.Archived;
        Check.Fails(() => item.MoveTo(MobilityTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        MobilityTierStartsInOpen();
        MobilityTierMovesToInReview();
        MobilityTierRejectsMovesFromArchived();
    }
}
