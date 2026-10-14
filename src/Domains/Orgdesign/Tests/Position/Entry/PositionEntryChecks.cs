namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PositionEntryChecks
{
    public static void PositionEntryStartsInPlanned()
    {
        var item = PositionEntry.NewPositionEntry("code-141", "label-492");
        Check.Equal(item.Status, PositionEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionEntryMovesToCompleted()
    {
        var item = PositionEntry.NewPositionEntry("code-141", "label-492");
        try
        {
            item.MoveTo(PositionEntryStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionEntryStatus.Completed, "status");
    }

    public static void PositionEntryRejectsMovesFromCancelled()
    {
        var item = PositionEntry.NewPositionEntry("code-141", "label-492");
        item.Status = PositionEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PositionEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PositionEntryStartsInPlanned();
        PositionEntryMovesToCompleted();
        PositionEntryRejectsMovesFromCancelled();
    }
}
