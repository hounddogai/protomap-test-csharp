namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgSlotChecks
{
    public static void ReorgSlotStartsInDraft()
    {
        var item = ReorgSlot.NewReorgSlot("code-576", "title-973");
        Check.Equal(item.Status, ReorgSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgSlotMovesToActive()
    {
        var item = ReorgSlot.NewReorgSlot("code-576", "title-973");
        try
        {
            item.MoveTo(ReorgSlotStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgSlotStatus.Active, "status");
    }

    public static void ReorgSlotRejectsMovesFromClosed()
    {
        var item = ReorgSlot.NewReorgSlot("code-576", "title-973");
        item.Status = ReorgSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(ReorgSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ReorgSlotStartsInDraft();
        ReorgSlotMovesToActive();
        ReorgSlotRejectsMovesFromClosed();
    }
}
