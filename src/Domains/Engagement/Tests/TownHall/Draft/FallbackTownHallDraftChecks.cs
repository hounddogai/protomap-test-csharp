namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackTownHallDraftChecks
{
    public static void FallbackTownHallDraftStartsInRequested()
    {
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("code-510", "label-59");
        Check.Equal(item.Status, FallbackTownHallDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackTownHallDraftMovesToRevoked()
    {
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("code-510", "label-59");
        try
        {
            item.MoveTo(FallbackTownHallDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackTownHallDraftStatus.Revoked, "status");
    }

    public static void FallbackTownHallDraftRejectsMovesFromExpired()
    {
        var item = FallbackTownHallDraft.NewFallbackTownHallDraft("code-510", "label-59");
        item.Status = FallbackTownHallDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackTownHallDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackTownHallDraftStartsInRequested();
        FallbackTownHallDraftMovesToRevoked();
        FallbackTownHallDraftRejectsMovesFromExpired();
    }
}
