namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LaptopSnapshotChecks
{
    public static void LaptopSnapshotStartsInDraft()
    {
        var item = LaptopSnapshot.NewLaptopSnapshot("code-291", "title-556");
        Check.Equal(item.Status, LaptopSnapshotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopSnapshotMovesToActive()
    {
        var item = LaptopSnapshot.NewLaptopSnapshot("code-291", "title-556");
        try
        {
            item.MoveTo(LaptopSnapshotStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopSnapshotStatus.Active, "status");
    }

    public static void LaptopSnapshotRejectsMovesFromClosed()
    {
        var item = LaptopSnapshot.NewLaptopSnapshot("code-291", "title-556");
        item.Status = LaptopSnapshotStatus.Closed;
        Check.Fails(() => item.MoveTo(LaptopSnapshotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LaptopSnapshotStartsInDraft();
        LaptopSnapshotMovesToActive();
        LaptopSnapshotRejectsMovesFromClosed();
    }
}
