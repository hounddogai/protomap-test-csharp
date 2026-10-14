namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskTierChecks
{
    public static void DeskTierStartsInProposed()
    {
        var item = DeskTier.NewDeskTier("code-790", "name-884");
        Check.Equal(item.Status, DeskTierStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskTierMovesToRejected()
    {
        var item = DeskTier.NewDeskTier("code-790", "name-884");
        try
        {
            item.MoveTo(DeskTierStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskTierStatus.Rejected, "status");
    }

    public static void DeskTierRejectsMovesFromWithdrawn()
    {
        var item = DeskTier.NewDeskTier("code-790", "name-884");
        item.Status = DeskTierStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeskTierStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeskTierStartsInProposed();
        DeskTierMovesToRejected();
        DeskTierRejectsMovesFromWithdrawn();
    }
}
