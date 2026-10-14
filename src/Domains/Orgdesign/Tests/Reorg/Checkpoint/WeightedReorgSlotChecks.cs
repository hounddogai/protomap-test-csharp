namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedReorgSlotChecks
{
    public static void WeightedReorgSlotStartsInOpen()
    {
        var item = WeightedReorgSlot.NewWeightedReorgSlot("code-453", "label-659");
        Check.Equal(item.Status, WeightedReorgSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedReorgSlotMovesToInReview()
    {
        var item = WeightedReorgSlot.NewWeightedReorgSlot("code-453", "label-659");
        try
        {
            item.MoveTo(WeightedReorgSlotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedReorgSlotStatus.InReview, "status");
    }

    public static void WeightedReorgSlotRejectsMovesFromArchived()
    {
        var item = WeightedReorgSlot.NewWeightedReorgSlot("code-453", "label-659");
        item.Status = WeightedReorgSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedReorgSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedReorgSlotStartsInOpen();
        WeightedReorgSlotMovesToInReview();
        WeightedReorgSlotRejectsMovesFromArchived();
    }
}
