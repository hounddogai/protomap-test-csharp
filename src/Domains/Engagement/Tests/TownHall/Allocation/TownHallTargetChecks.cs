namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallTargetChecks
{
    public static void TownHallTargetStartsInCreated()
    {
        var item = TownHallTarget.NewTownHallTarget("code-734", "title-560");
        Check.Equal(item.Status, TownHallTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallTargetMovesToPublished()
    {
        var item = TownHallTarget.NewTownHallTarget("code-734", "title-560");
        try
        {
            item.MoveTo(TownHallTargetStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallTargetStatus.Published, "status");
    }

    public static void TownHallTargetRejectsMovesFromRetired()
    {
        var item = TownHallTarget.NewTownHallTarget("code-734", "title-560");
        item.Status = TownHallTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(TownHallTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        TownHallTargetStartsInCreated();
        TownHallTargetMovesToPublished();
        TownHallTargetRejectsMovesFromRetired();
    }
}
