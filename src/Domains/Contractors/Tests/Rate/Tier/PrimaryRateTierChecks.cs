namespace Acme.Workforce.Contractors.Tests.Rate.Tier;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PrimaryRateTierChecks
{
    public static void PrimaryRateTierStartsInOpen()
    {
        var item = PrimaryRateTier.NewPrimaryRateTier("code-353", "title-235");
        Check.Equal(item.Status, PrimaryRateTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRateTierMovesToInReview()
    {
        var item = PrimaryRateTier.NewPrimaryRateTier("code-353", "title-235");
        try
        {
            item.MoveTo(PrimaryRateTierStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRateTierStatus.InReview, "status");
    }

    public static void PrimaryRateTierRejectsMovesFromArchived()
    {
        var item = PrimaryRateTier.NewPrimaryRateTier("code-353", "title-235");
        item.Status = PrimaryRateTierStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryRateTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryRateTierStartsInOpen();
        PrimaryRateTierMovesToInReview();
        PrimaryRateTierRejectsMovesFromArchived();
    }
}
