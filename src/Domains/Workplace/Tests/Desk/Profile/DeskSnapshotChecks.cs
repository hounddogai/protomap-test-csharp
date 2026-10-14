namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskSnapshotChecks
{
    public static void DeskSnapshotStartsInDraft()
    {
        var item = DeskSnapshot.NewDeskSnapshot("code-931", "label-799");
        Check.Equal(item.Status, DeskSnapshotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskSnapshotMovesToActive()
    {
        var item = DeskSnapshot.NewDeskSnapshot("code-931", "label-799");
        try
        {
            item.MoveTo(DeskSnapshotStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskSnapshotStatus.Active, "status");
    }

    public static void DeskSnapshotRejectsMovesFromClosed()
    {
        var item = DeskSnapshot.NewDeskSnapshot("code-931", "label-799");
        item.Status = DeskSnapshotStatus.Closed;
        Check.Fails(() => item.MoveTo(DeskSnapshotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DeskSnapshotStartsInDraft();
        DeskSnapshotMovesToActive();
        DeskSnapshotRejectsMovesFromClosed();
    }
}
