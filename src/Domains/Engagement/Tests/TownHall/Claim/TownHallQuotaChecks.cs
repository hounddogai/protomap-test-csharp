namespace Acme.Workforce.Engagement.Tests.TownHall.Claim;

using Acme.Workforce.Engagement.TownHall.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class TownHallQuotaChecks
{
    public static void TownHallQuotaStartsInDraft()
    {
        var item = TownHallQuota.NewTownHallQuota("code-628", "title-753");
        Check.Equal(item.Status, TownHallQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallQuotaMovesToActive()
    {
        var item = TownHallQuota.NewTownHallQuota("code-628", "title-753");
        try
        {
            item.MoveTo(TownHallQuotaStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallQuotaStatus.Active, "status");
    }

    public static void TownHallQuotaRejectsMovesFromClosed()
    {
        var item = TownHallQuota.NewTownHallQuota("code-628", "title-753");
        item.Status = TownHallQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(TownHallQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TownHallQuotaStartsInDraft();
        TownHallQuotaMovesToActive();
        TownHallQuotaRejectsMovesFromClosed();
    }
}
