namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TieredPositionAdjustmentChecks
{
    public static void TieredPositionAdjustmentStartsInDraft()
    {
        var item = TieredPositionAdjustment.NewTieredPositionAdjustment("code-304", "name-511");
        Check.Equal(item.Status, TieredPositionAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPositionAdjustmentMovesToPending()
    {
        var item = TieredPositionAdjustment.NewTieredPositionAdjustment("code-304", "name-511");
        try
        {
            item.MoveTo(TieredPositionAdjustmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPositionAdjustmentStatus.Pending, "status");
    }

    public static void TieredPositionAdjustmentRejectsMovesFromClosed()
    {
        var item = TieredPositionAdjustment.NewTieredPositionAdjustment("code-304", "name-511");
        item.Status = TieredPositionAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredPositionAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredPositionAdjustmentStartsInDraft();
        TieredPositionAdjustmentMovesToPending();
        TieredPositionAdjustmentRejectsMovesFromClosed();
    }
}
