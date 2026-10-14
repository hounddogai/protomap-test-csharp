namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSlotChecks
{
    public static void PositionSlotStartsInNew()
    {
        var item = PositionSlot.NewPositionSlot("code-319", "name-598");
        Check.Equal(item.Status, PositionSlotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionSlotMovesToQueued()
    {
        var item = PositionSlot.NewPositionSlot("code-319", "name-598");
        try
        {
            item.MoveTo(PositionSlotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionSlotStatus.Queued, "status");
    }

    public static void PositionSlotRejectsMovesFromFailed()
    {
        var item = PositionSlot.NewPositionSlot("code-319", "name-598");
        item.Status = PositionSlotStatus.Failed;
        Check.Fails(() => item.MoveTo(PositionSlotStatus.New), "move");
    }

    public static void RunAll()
    {
        PositionSlotStartsInNew();
        PositionSlotMovesToQueued();
        PositionSlotRejectsMovesFromFailed();
    }
}
